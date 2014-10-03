using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GoToWindow.Api;
using Microsoft.Win32;
using log4net;
using GoToWindow.Squirrel;

namespace GoToWindow.ViewModels
{
	public enum CheckForUpdatesStatus
	{
		Undefined,
		Checking,
		UpdateAvailable,
		AlreadyUpToDate,
		Error
	}

	public enum ShortcutPresets
	{
		Undefined,
		Disabled,
		AltTab,
		AltTabTab,
		WinTab,
		Custom
	}

	public enum ShortcutControlKeys
	{
		Undefined = 0,
		Ctrl = 0xA2, //VK_LCONTROL
		Alt = 0xA4, //VK_LMENU,
		Win = 0x5B //VK_LWIN
	}

	public enum ShortcutKeys
	{
		Undefined = 0,
		Tab = 0x09, //VK_TAB
		Console = 0xC0, //~
		Escape = 0x1B //VK_ESCAPE
	}

	public class SettingsViewModel : NotifyPropertyChangedViewModelBase
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsViewModel).Assembly, "GoToWindow");

		private readonly IGoToWindowContext _context;
		private readonly SquirrelUpdater _updater;

		private bool _originalStartWithWindows;

		protected SettingsViewModel()
		{

		}

	    public SettingsViewModel(IGoToWindowContext context)
		{
			_context = context;
			_enabled = true;
			_updater = SquirrelContext.AcquireUpdater();

			Load();
		}

		public bool StartWithWindows { get; set; }
        public int ShortcutPressesBeforeOpen { get; set; }
        public bool WindowListSingleClick { get; set; }
		public bool NoElevatedPrivilegesWarning { get; set; }
		public string Version { get; set; }
		public List<SettingsPluginViewModel> Plugins { get; protected set; }

		private string _latestAvailableRelease;
		public string LatestAvailableRelease
		{
			get { return _latestAvailableRelease; }
			set
			{
				_latestAvailableRelease = value;
				OnPropertyChanged("LatestAvailableRelease");
			}
		}

		private CheckForUpdatesStatus _updateAvailable;
		public CheckForUpdatesStatus UpdateAvailable
		{
			get { return _updateAvailable; }
			set
			{
				_updateAvailable = value;
				OnPropertyChanged("UpdateAvailable");
			}
		}

		private bool _enabled;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				OnPropertyChanged("Enabled");
			}
		}

		private ShortcutPresets _shortcutPreset;
		public ShortcutPresets ShortcutPreset
		{
			get { return _shortcutPreset; }
			set
			{
				_shortcutPreset = value;
				OnPropertyChanged("ShortcutPreset");
				UpdateShortcutValidity();
			}
		}

		private ShortcutControlKeys _shortcutControlKey1;
		public ShortcutControlKeys ShortcutControlKey1
		{
			get { return _shortcutControlKey1; }
			set
			{
				_shortcutControlKey1 = value;
				OnPropertyChanged("ShortcutControlKey1");
				UpdateShortcutValidity();
			}
		}

		private ShortcutKeys _shortcutKey;
		public ShortcutKeys ShortcutKey
		{
			get { return _shortcutKey; }
			set
			{
				_shortcutKey = value;
				OnPropertyChanged("ShortcutKey");
				UpdateShortcutValidity();
			}
		}

		private bool _isShortcutInvalid;
		public bool IsShortcutInvalid
		{
			get { return _isShortcutInvalid; }
			set
			{
				_isShortcutInvalid = value;
				OnPropertyChanged("IsShortcutInvalid");
			}
		}

		public void Load()
		{
			// Settings
			StartWithWindows = _originalStartWithWindows = GetStartWithWindows();
		    WindowListSingleClick = Properties.Settings.Default.WindowListSingleClick;

			// Shortcut
			var shortcut = KeyboardShortcut.FromString(GoToWindow.Properties.Settings.Default.OpenShortcut);
			ShortcutPreset = ShortcutPresets.Custom; //TODO
			ShortcutControlKey1 = Enum.IsDefined(typeof(ShortcutControlKeys), shortcut.ControlVirtualKeyCode) ? (ShortcutControlKeys)shortcut.ControlVirtualKeyCode : ShortcutControlKeys.Undefined;
			ShortcutKey = Enum.IsDefined(typeof(ShortcutKeys), shortcut.VirtualKeyCode) ? (ShortcutKeys)shortcut.VirtualKeyCode : ShortcutKeys.Undefined;
			ShortcutPressesBeforeOpen = shortcut.ShortcutPressesBeforeOpen;

			// Warnings
			NoElevatedPrivilegesWarning = !WindowsRuntimeHelper.GetHasElevatedPrivileges();

			// Plugins
			var disabledPlugins = Properties.Settings.Default.DisabledPlugins ?? new StringCollection();

			Plugins = _context.PluginsContainer.Plugins
				.Select(plugin => new SettingsPluginViewModel
					{
						Id = plugin.Id,
						Enabled = !disabledPlugins.Contains(plugin.Id),
						Name = plugin.Title
					})
				.OrderBy(plugin => plugin.Name)
				.ToList();
			var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
			Version = String.Format("{0}.{1}.{2}", currentVersion.Major, currentVersion.Minor, currentVersion.Build);

			// Updates
			UpdateAvailable = CheckForUpdatesStatus.Checking;
			_updater.CheckForUpdates(CheckForUpdatesCallback, CheckForUpdatesError);
		}

		public void Apply()
		{
			// Update Registry
			if (_originalStartWithWindows != StartWithWindows)
			{
				UpdateStartWithWindows(StartWithWindows);
			}

			// Update Shortcut
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = (int)ShortcutControlKey1,
				VirtualKeyCode = (int)ShortcutKey,
				ShortcutPressesBeforeOpen = ShortcutPressesBeforeOpen
			};
			Properties.Settings.Default.OpenShortcut = shortcut.ToString();
			_context.EnableAltTabHook(shortcut);

			// Settings
			Properties.Settings.Default.WindowListSingleClick = WindowListSingleClick;

			// Plugins
			var disabledPlugins = new StringCollection();
			disabledPlugins.AddRange(Plugins.Where(plugin => !plugin.Enabled).Select(plugin => plugin.Id).ToArray());
			Properties.Settings.Default.DisabledPlugins = disabledPlugins;

			// Save
			Properties.Settings.Default.Save();
			Log.InfoFormat("Settings updated. Shortcut is '{0}'", shortcut.ToString());
		}

		private void CheckForUpdatesCallback(string latestVersion)
		{
			UpdateAvailable = latestVersion != null ? CheckForUpdatesStatus.UpdateAvailable : CheckForUpdatesStatus.AlreadyUpToDate;
			LatestAvailableRelease = latestVersion;
		}

		private void CheckForUpdatesError(Exception exc)
		{
			UpdateAvailable = CheckForUpdatesStatus.Error;
			Enabled = true;
		}

		private void UpdateShortcutValidity()
		{
			IsShortcutInvalid = !CheckShortcutValidity();
		}

		private bool CheckShortcutValidity()
		{
			if (ShortcutPreset == ShortcutPresets.Undefined)
				return false;

			if (ShortcutPreset != ShortcutPresets.Custom)
				return true;

			if (ShortcutControlKey1 == ShortcutControlKeys.Undefined)
				return false;

			if (ShortcutKey == ShortcutKeys.Undefined)
				return false;

			return true;
		}

		private static bool GetStartWithWindows()
		{
			var runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
		    
            if (runList == null) return false;
		    
            var executablePath = Assembly.GetExecutingAssembly().Location;
		    return ((string)runList.GetValue("GoToWindow") == executablePath);
		}

		private static void UpdateStartWithWindows(bool active)
		{
			if (active)
			{
				var executablePath = Assembly.GetExecutingAssembly().Location;

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "reg.exe",
						Arguments = string.Format("add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /t REG_SZ /d \"{0}\" /f", executablePath),
						Verb = "runas",
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden

					}
				};
				process.Start();
				process.WaitForExit();
			}
			else
			{
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "reg.exe",
						Arguments = "delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /f",
						Verb = "runas",
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden
					}
				};
				process.Start();
				process.WaitForExit();
			}
		}
	}
}
