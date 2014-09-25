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
		Downloading,
		Installing,
		Restarting,
		Error
	}

	public static class SquirrelContext
	{
		private static readonly SquirrelUpdater Updater = new SquirrelUpdater();

		public static SquirrelUpdater AcquireUpdater()
		{
			return Updater;
		}
	}

	public class SquirrelUpdater : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SquirrelUpdater).Assembly, "GoToWindow");
		private IUpdateManager _updateManager;
		private UpdateInfo _updateInfo;

		public void CheckForUpdates(Action<string> callback, Action<Exception> errCallback)
		{
			if (_updateManager != null)
				return;

			_updateManager = new UpdateManager(@"D:\Dev\GoToWindow\Releases", "GoToWindow", FrameworkVersion.Net45);
			var checkForUpdateTask = _updateManager.CheckForUpdate();
			checkForUpdateTask.ContinueWith(t => CheckForUpdateCallback(callback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			checkForUpdateTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CheckForUpdateCallback(Action<string> callback, UpdateInfo updateInfo)
		{
			if (updateInfo == null)
			{
				DisposeUpdateManager();
				callback(null);
			}
			else if (!updateInfo.ReleasesToApply.Any())
			{
				DisposeUpdateManager();
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
			Log.Info("Squirrel: Update complete.");
			DisposeUpdateManager();

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
			catch(Exception exc)
			{
				HandleAsyncError(errCallback, exc);
			}
		}

		private void HandleAsyncError(Action<Exception> errCallback, Exception exc)
		{
			Log.Error("Error while trying to check for updates", exc);
			DisposeUpdateManager();
			errCallback(exc);
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
