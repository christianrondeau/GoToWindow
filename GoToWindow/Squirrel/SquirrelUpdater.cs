using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Squirrel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GoToWindow.Squirrel
{
	public enum UpdateStatus
	{
		Undefined,
		Checking,
		UpdateAvailable,
		AlreadyUpToDate,
		Updating,
		Error,
		UpdateDownloading,
		Restarting
	}

	public class SquirrelUpdater : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SquirrelUpdater).Assembly, "GoToWindow");
		private IUpdateManager _updateManager;
		private UpdateInfo _updateInfo;

		public void CheckForUpdates(Action<UpdateStatus, string> callback)
		{
			if (_updateManager != null)
				return;

			_updateManager = new UpdateManager(@"D:\Dev\GoToWindow\Releases", "GoToWindow", FrameworkVersion.Net45);
			var checkForUpdateTask = _updateManager.CheckForUpdate();
			checkForUpdateTask.ContinueWith(t => CheckForUpdateCallback(callback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			checkForUpdateTask.ContinueWith(t => HandleAsyncError(callback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CheckForUpdateCallback(Action<UpdateStatus, string> callback, UpdateInfo updateInfo)
		{
			if (updateInfo == null)
			{
				DisposeUpdateManager();
				callback(UpdateStatus.Undefined, null);
			}
			else if (!updateInfo.ReleasesToApply.Any())
			{
				DisposeUpdateManager();
				callback(UpdateStatus.AlreadyUpToDate, null);
			}
			else
			{
				_updateInfo = updateInfo;
				var latest = _updateInfo.ReleasesToApply.OrderBy(x => x.Version).Last();
				callback(UpdateStatus.UpdateAvailable, latest.Version.ToString());
			}
		}

		public void Update(Action<UpdateStatus, string> callback)
		{
			if (_updateInfo == null)
				throw new ApplicationException("The update available link was shown while update info is not available to SettingsViewModel.Update.");

			// TODO: Make an update dialog
			callback(UpdateStatus.UpdateDownloading, null);
			Log.Info("Squirrel: Downloading releases...");
			var downloadReleasesTask = _updateManager.DownloadReleases(_updateInfo.ReleasesToApply);
			downloadReleasesTask.ContinueWith(t => DownloadReleasesCallback(callback), TaskContinuationOptions.OnlyOnRanToCompletion);
			downloadReleasesTask.ContinueWith(t => HandleAsyncError(callback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void DownloadReleasesCallback(Action<UpdateStatus, string> callback)
		{
			callback(UpdateStatus.Updating, null);
			Log.Info("Squirrel: Applying releases...");
			var applyReleasesTask = _updateManager.ApplyReleases(_updateInfo);
			applyReleasesTask.ContinueWith(t => ApplyReleasesCallback(callback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			applyReleasesTask.ContinueWith(t => HandleAsyncError(callback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ApplyReleasesCallback(Action<UpdateStatus, string> callback, string installPath)
		{
			Log.Info("Squirrel: Update complete.");
			DisposeUpdateManager();

			Log.Info("Squirrel: Launching new version.");
			callback(UpdateStatus.Restarting, null);
			
			var executablePath = Path.Combine(installPath, "GoToWindow.exe");
			if (File.Exists(executablePath))
			{
				Process.Start(executablePath, "--squirrel-firstrunafterupdate");
			}

			Log.Info("Squirrel: Shutting down.");
			Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown(1));
		}

		private void HandleAsyncError(Action<UpdateStatus, string> callback, Exception exc)
		{
			Log.Error("Error while trying to check for updates", exc);
			DisposeUpdateManager();
			callback(UpdateStatus.Error, null);
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
