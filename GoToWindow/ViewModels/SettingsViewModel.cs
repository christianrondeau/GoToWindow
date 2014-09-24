using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using GoToWindow.Api;
using Microsoft.Win32;
using Squirrel;
using log4net;
using System.Threading.Tasks;
using System.IO;
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

	public class SettingsViewModel : NotifyPropertyChangedViewModelBase
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsViewModel).Assembly, "GoToWindow");

		private readonly IGoToWindowContext _context;
		private SquirrelUpdater _updater;

		private bool _originalHookAltTab;
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

		public bool HookAltTab { get; set; }
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

		public void Load()
		{
			HookAltTab = _originalHookAltTab = Properties.Settings.Default.HookAltTab;
			StartWithWindows = _originalStartWithWindows = GetStartWithWindows();
			ShortcutPressesBeforeOpen = Properties.Settings.Default.ShortcutPressesBeforeOpen;
		    WindowListSingleClick = Properties.Settings.Default.WindowListSingleClick;

			NoElevatedPrivilegesWarning = !WindowsRuntimeHelper.GetHasElevatedPrivileges();

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

			UpdateAvailable = CheckForUpdatesStatus.Checking;
			_updater.CheckForUpdates(CheckForUpdatesCallback, CheckForUpdatesError);
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

		private static bool GetStartWithWindows()
		{
			var runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
		    
            if (runList == null) return false;
		    
            var executablePath = Assembly.GetExecutingAssembly().Location;
		    return ((string)runList.GetValue("GoToWindow") == executablePath);
		}

		public void Apply()
		{
			if (_originalStartWithWindows != StartWithWindows)
			{
				UpdateStartWithWindows(StartWithWindows);
            }

            Properties.Settings.Default.HookAltTab = HookAltTab;
            Properties.Settings.Default.ShortcutPressesBeforeOpen = ShortcutPressesBeforeOpen;
            Properties.Settings.Default.WindowListSingleClick = WindowListSingleClick;

			if(_originalHookAltTab != HookAltTab)
			{
				_context.EnableAltTabHook(HookAltTab, ShortcutPressesBeforeOpen);
			}

			var disabledPlugins = new StringCollection();
			disabledPlugins.AddRange(Plugins.Where(plugin => !plugin.Enabled).Select(plugin => plugin.Id).ToArray());
			Properties.Settings.Default.DisabledPlugins = disabledPlugins;

			Properties.Settings.Default.Save();
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
