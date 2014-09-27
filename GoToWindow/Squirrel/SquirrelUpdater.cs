using System;
using System.Linq;
using System.Threading.Tasks;
using GoToWindow.ViewModels;
using GoToWindow.Windows;
using log4net;
using Squirrel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Reflection;

namespace GoToWindow.Squirrel
{
	public enum UpdateStatus
	{
		Undefined,
		Downloading,
		Installing,
		Restarting,
		Error
	}

	public static class SquirrelContext
	{
		private static readonly SquirrelUpdater Updater = new SquirrelUpdater(GetUpdateUrl());

		public static SquirrelUpdater AcquireUpdater()
		{
			return Updater;
		}

        public static void Dispose()
        {
            Updater.Dispose();
        }

		static string GetUpdateUrl()
		{
			var args = Environment.GetCommandLineArgs();
			var updateUrlArgumentIndex = Array.IndexOf(args, "-updateUrl");
			if (updateUrlArgumentIndex != -1 && args.Length > updateUrlArgumentIndex + 1)
			{
				var updateUrl = args[updateUrlArgumentIndex + 1];

				if (!Uri.IsWellFormedUriString(updateUrl, UriKind.Absolute))
				{
					var appPath = Path.GetFileName(Assembly.GetEntryAssembly().Location);
					var dirPath = Path.GetDirectoryName(appPath);
					return Path.Combine(dirPath, updateUrl);
				}

				return updateUrl;
			}

			return "http://christianrondeau.github.io/GoToWindow/Releases";
		}
	}

	public class SquirrelUpdater : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SquirrelUpdater).Assembly, "GoToWindow");
		private IUpdateManager _updateManager;
		private UpdateInfo _updateInfo;

		public SquirrelUpdater(string updateUrl)
		{
			_updateManager = new UpdateManager(updateUrl, "GoToWindow", FrameworkVersion.Net45);
		}

		public void CheckForUpdates(Action<string> callback, Action<Exception> errCallback)
		{
			var checkForUpdateTask = _updateManager.CheckForUpdate();
			checkForUpdateTask.ContinueWith(t => CheckForUpdateCallback(callback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			checkForUpdateTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CheckForUpdateCallback(Action<string> callback, UpdateInfo updateInfo)
		{
			if (updateInfo == null)
			{
				callback(null);
			}
			else if (!updateInfo.ReleasesToApply.Any())
			{
				callback(null);
			}
			else
			{
				_updateInfo = updateInfo;
				var latest = _updateInfo.ReleasesToApply.OrderBy(x => x.Version).Last();
				callback(latest.Version.ToString());
			}
		}

		public void Update(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback)
		{
			if (_updateInfo == null)
				throw new ApplicationException("The update available link was shown while update info is not available to SettingsViewModel.Update.");

			// TODO: Make an update dialog
			progressCallback(UpdateStatus.Downloading, 0);
			Log.Info("Squirrel: Downloading releases...");
			var downloadReleasesTask = _updateManager.DownloadReleases(_updateInfo.ReleasesToApply, progress => progressCallback(UpdateStatus.Downloading, progress));
			downloadReleasesTask.ContinueWith(t => DownloadReleasesCallback(progressCallback, errCallback), TaskContinuationOptions.OnlyOnRanToCompletion);
			downloadReleasesTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void DownloadReleasesCallback(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback)
		{
			progressCallback(UpdateStatus.Installing, 0);
			Log.Info("Squirrel: Applying releases...");
			var applyReleasesTask = _updateManager.ApplyReleases(_updateInfo, progress => progressCallback(UpdateStatus.Installing, progress));
			applyReleasesTask.ContinueWith(t => ApplyReleasesCallback(progressCallback, errCallback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			applyReleasesTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ApplyReleasesCallback(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback, string installPath)
		{
            progressCallback(UpdateStatus.Installing, 100);
            Log.Info("Squirrel: Creating uninstall info...");
            var createUninstallerRegistryEntryTask = _updateManager.CreateUninstallerRegistryEntry();
            createUninstallerRegistryEntryTask.ContinueWith(t => CreateUninstallerRegistryEntryCallback(progressCallback, errCallback, installPath), TaskContinuationOptions.OnlyOnRanToCompletion);
            createUninstallerRegistryEntryTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

        private void CreateUninstallerRegistryEntryCallback(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback, string installPath)
        {
            _updateManager.Dispose();
            Log.Info("Squirrel: Launching new version.");
            progressCallback(UpdateStatus.Restarting, 100);

            try
            {
                var executablePath = Path.Combine(installPath, "GoToWindow.exe");
                if (File.Exists(executablePath))
                {
                    Process.Start(executablePath, "--squirrel-firstrunafterupdate");
                }

                Log.Info("Squirrel: Shutting down.");
                Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown(1));
            }
            catch (Exception exc)
            {
                HandleAsyncError(errCallback, exc);
            }
        }

		private void HandleAsyncError(Action<Exception> errCallback, Exception exc)
		{
			Log.Error("Error while trying to check for updates", exc);
			
			if (errCallback != null)
				errCallback(exc);
		}

		public void Dispose()
		{
			if (_updateManager == null)
				return;

			_updateManager.Dispose();
			_updateManager = null;
		}

		public static void ShowUpdateWindow()
		{
			var updateWindow = new UpdateWindow();
			var updateViewModel = new UpdateViewModel();
			updateWindow.DataContext = updateViewModel;
			updateViewModel.Update();
			updateWindow.ShowDialog();
		}

		internal void InstallShortcuts(bool updateOnly)
		{
			_updateManager.CreateShortcutsForExecutable(
				Path.GetFileName(Assembly.GetEntryAssembly().Location),
				ShortcutLocation.Desktop | ShortcutLocation.StartMenu,
				updateOnly
				);
		}

		internal void RemoveShortcuts()
		{
			_updateManager.RemoveShortcutsForExecutable(
				Path.GetFileName(Assembly.GetEntryAssembly().Location),
				ShortcutLocation.Desktop | ShortcutLocation.StartMenu);
		}
	}
}
