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

namespace GoToWindow.ViewModels
{
	public enum UpdateStatus
	{
		Undefined,
		Checking,
		UpdateAvailable,
		AlreadyUpToDate,
		Updating,
		UpdateComplete,
		Error
	}

	public class SettingsViewModel : NotifyPropertyChangedViewModelBase, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsViewModel).Assembly, "GoToWindow");

		private readonly IGoToWindowContext _context;
		private IUpdateManager _updateManager;
		private UpdateInfo _updateInfo;

		private bool _originalHookAltTab;
		private bool _originalStartWithWindows;

		protected SettingsViewModel()
		{

		}

	    public SettingsViewModel(IGoToWindowContext context)
		{
			_context = context;
			_enabled = true;

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

		private UpdateStatus _updateAvailable;
		public UpdateStatus UpdateAvailable
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

			_updateManager = new UpdateManager(@"D:\Dev\GoToWindow\Releases", "GoToWindow", FrameworkVersion.Net45);
			UpdateAvailable = UpdateStatus.Checking;
			var checkForUpdateTask = _updateManager.CheckForUpdate();
			checkForUpdateTask.ContinueWith(t => CheckForUpdateCallback(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			checkForUpdateTask.ContinueWith(t => HandleAsyncError(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CheckForUpdateCallback(UpdateInfo updateInfo)
		{
			if (updateInfo == null)
			{
				UpdateAvailable = UpdateStatus.Undefined;
				DisposeUpdateManager();
			}
			else if (!updateInfo.ReleasesToApply.Any())
			{
				UpdateAvailable = UpdateStatus.AlreadyUpToDate;
				DisposeUpdateManager();
			}
			else
			{
				_updateInfo = updateInfo;
				var latest = _updateInfo.ReleasesToApply.OrderBy(x => x.Version).Last();
				LatestAvailableRelease = latest.Version.ToString();
				UpdateAvailable = UpdateStatus.UpdateAvailable;
			}
		}

		private void HandleAsyncError(Exception exc)
		{
			Log.Error("Error while trying to check for updates", exc);
			UpdateAvailable = UpdateStatus.Error;
			DisposeUpdateManager();
			Enabled = true;
		}

		public void Update()
		{
			if (_updateInfo == null)
			{
				Log.Warn("The update available link was shown while update info is not available to SettingsViewModel.Update.");
				MessageBox.Show("No update available to install. You can try downloading and re-installing the latest version manually.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
				UpdateAvailable = UpdateStatus.Error;
				return;
			}

			Enabled = false;
			UpdateAvailable = UpdateStatus.Updating;
			// TODO: Make an update dialog
			Log.Info("Squirrel: Downloading releases...");
			var downloadReleasesTask = _updateManager.DownloadReleases(_updateInfo.ReleasesToApply);
			downloadReleasesTask.ContinueWith(t => DownloadReleasesCallback(), TaskContinuationOptions.OnlyOnRanToCompletion);
			downloadReleasesTask.ContinueWith(t => HandleAsyncError(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void DownloadReleasesCallback()
		{
			Log.Info("Squirrel: Applying releases...");
			var applyReleasesTask = _updateManager.ApplyReleases(_updateInfo);
			applyReleasesTask.ContinueWith(t => ApplyReleasesCallback(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			applyReleasesTask.ContinueWith(t => HandleAsyncError(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ApplyReleasesCallback(string installPath)
		{
			// TODO: Since we now have two instances of GoToWindow running, we should exit this one and the new one should wait?
			Log.Info("Squirrel: Update complete.");
			DisposeUpdateManager();
			UpdateAvailable = UpdateStatus.UpdateComplete;

			Log.Info("Squirrel: Launching new version.");

			var executablePath = Path.Combine(installPath, "GoToWindow.exe");
			if (File.Exists(executablePath))
			{
				Process.Start(executablePath, "--squirrel-firstrunafterupdate");
			}

			Log.Info("Squirrel: Shutting down.");
			Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown(1));
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

		public void Dispose()
		{
			DisposeUpdateManager();
		}

		private void DisposeUpdateManager()
		{
			if (_updateManager == null)
				return;

			_updateManager.Dispose();
			_updateManager = null;
		}
	}
}
