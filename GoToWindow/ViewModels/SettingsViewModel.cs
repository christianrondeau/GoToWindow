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

	public class SettingsViewModel : NotifyPropertyChangedViewModelBase
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsViewModel).Assembly, "GoToWindow");

		private readonly IGoToWindowContext _context;
		private readonly SquirrelUpdater _updater;


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

		private KeyboardControlKeys _shortcutControlKey1;
		public KeyboardControlKeys ShortcutControlKey1
		{
			get { return _shortcutControlKey1; }
			set
			{
				_shortcutControlKey1 = value;
				OnPropertyChanged("ShortcutControlKey1");
				UpdateShortcutValidity();
			}
		}

		private KeyboardVirtualKeys _shortcutKey;
		public KeyboardVirtualKeys ShortcutKey
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
		    WindowListSingleClick = Properties.Settings.Default.WindowListSingleClick;

			// Shortcut
			var shortcut = KeyboardShortcut.FromString(GoToWindow.Properties.Settings.Default.OpenShortcut);
			ShortcutPreset = ShortcutPresets.Custom; //TODO
			ShortcutControlKey1 = Enum.IsDefined(typeof(KeyboardControlKeys), shortcut.ControlVirtualKeyCode) ? (KeyboardControlKeys)shortcut.ControlVirtualKeyCode : KeyboardControlKeys.Undefined;
			ShortcutKey = Enum.IsDefined(typeof(KeyboardVirtualKeys), shortcut.VirtualKeyCode) ? (KeyboardVirtualKeys)shortcut.VirtualKeyCode : KeyboardVirtualKeys.Undefined;
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

			if (ShortcutControlKey1 == KeyboardControlKeys.Undefined)
				return false;

			if (ShortcutKey == KeyboardVirtualKeys.Undefined)
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
	}
}
