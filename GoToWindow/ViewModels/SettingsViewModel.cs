using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using GoToWindow.Api;
using GoToWindow.Properties;
using GoToWindow.Squirrel;
using log4net;
using Microsoft.Win32;

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

		public bool WindowListSingleClick { get; set; }
		public bool NoElevatedPrivilegesWarning { get; set; }
		public string Version { get; set; }
		public List<SettingsPluginViewModel> Plugins { get; protected set; }

		private int _shortcutPressesBeforeOpen;
		public int ShortcutPressesBeforeOpen
		{
			get { return _shortcutPressesBeforeOpen; }
			set
			{
				_shortcutPressesBeforeOpen = value;
				UpdateShortcutDescription();
			}
		}

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

		private KeyboardControlKeys _shortcutControlKey;
		public KeyboardControlKeys ShortcutControlKey
		{
			get { return _shortcutControlKey; }
			set
			{
				_shortcutControlKey = value;
				OnPropertyChanged("ShortcutControlKey");
				UpdateShortcutValidity();
				UpdateShortcutDescription();
			}
		}

		private KeyboardVirtualKeys _shortcutKeyPreset;
		public KeyboardVirtualKeys ShortcutKeyPreset
		{
			get { return _shortcutKeyPreset; }
			set
			{
				_shortcutKeyPreset = value;
				if (value == KeyboardVirtualKeys.Custom)
					ShortcutKey = (int) KeyboardVirtualKeys.Tab;
				OnPropertyChanged("ShortcutKeyPreset");
				UpdateShortcutDescription();
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

		private int _shortcutKey;
		public int ShortcutKey
		{
			get { return _shortcutKey; }
			set
			{
				_shortcutKey = value;
				OnPropertyChanged("ShortcutKey");
				UpdateShortcutValidity();
				UpdateShortcutDescription();
			}
		}

		private bool _showCustomShortcutKey;
		public bool ShowCustomShortcutKey
		{
			get { return _showCustomShortcutKey; }
			set
			{
				_showCustomShortcutKey = value;
				OnPropertyChanged("ShowCustomShortcutKey");
			}
		}

		public string ShortcutDescription
		{
			get { return CreateShortcut().ToHumanReadableString(); }
		}

		public void UpdateShortcutDescription()
		{
			OnPropertyChanged("ShortcutDescription");
		}

		public void Load()
		{
			// Settings
		    WindowListSingleClick = Settings.Default.WindowListSingleClick;

			// Shortcut
			var shortcut = KeyboardShortcut.FromString(Settings.Default.OpenShortcut);
			ShortcutControlKey = Enum.IsDefined(typeof(KeyboardControlKeys), shortcut.ControlVirtualKeyCode) ? (KeyboardControlKeys)shortcut.ControlVirtualKeyCode : KeyboardControlKeys.Undefined;
			ShortcutKeyPreset = Enum.IsDefined(typeof(KeyboardVirtualKeys), shortcut.VirtualKeyCode) ? (KeyboardVirtualKeys)shortcut.VirtualKeyCode : KeyboardVirtualKeys.Custom;
			ShortcutKey = shortcut.VirtualKeyCode;
			ShortcutPressesBeforeOpen = shortcut.ShortcutPressesBeforeOpen;

			// Warnings
			NoElevatedPrivilegesWarning = !WindowsRuntimeHelper.GetHasElevatedPrivileges();

			// Plugins
			var disabledPlugins = Settings.Default.DisabledPlugins ?? new StringCollection();

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
			var shortcut = CreateShortcut();
			Settings.Default.OpenShortcut = shortcut.ToString();
			_context.EnableKeyboardHook(shortcut);

			// Settings
			Settings.Default.WindowListSingleClick = WindowListSingleClick;

			// Plugins
			var disabledPlugins = new StringCollection();
			disabledPlugins.AddRange(Plugins.Where(plugin => !plugin.Enabled).Select(plugin => plugin.Id).ToArray());
			Settings.Default.DisabledPlugins = disabledPlugins;

			// Save
			Settings.Default.Save();
			Log.InfoFormat("Settings updated. Shortcut is '{0}'", shortcut.ToString());
		}

		private KeyboardShortcut CreateShortcut()
		{
			return new KeyboardShortcut
			{
				ControlVirtualKeyCode = (int)ShortcutControlKey,
				VirtualKeyCode = ShortcutKey,
				ShortcutPressesBeforeOpen = ShortcutPressesBeforeOpen
			};
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
			if (ShortcutControlKey == KeyboardControlKeys.Undefined)
				return false;

			if (ShortcutKey == 0)
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
