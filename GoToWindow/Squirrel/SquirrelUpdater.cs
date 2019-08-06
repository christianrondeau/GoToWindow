﻿using System;
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

		private static string GetUpdateUrl()
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
					if (dirPath == null)
						throw new ApplicationException($"Could not get the directory name from path {appPath}");
					return Path.Combine(dirPath, updateUrl);
				}

				return updateUrl;
			}

			return "http://christianrondeau.github.io/GoToWindow/Releases";
		}
	}

	public class SquirrelUpdater : IDisposable
	{
		public static readonly bool Enabled;
		private static readonly ILog Log;
		private static readonly string ExecutableFilename;

		private IUpdateManager _updateManager;
		private UpdateInfo _updateInfo;

		static SquirrelUpdater()
		{
			Log = LogManager.GetLogger(typeof(SquirrelUpdater).Assembly, "GoToWindow");

			var executablePath = Assembly.GetEntryAssembly().Location;
			ExecutableFilename = Path.GetFileName(executablePath);

			var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			Enabled = executablePath.StartsWith(appDataPath, StringComparison.InvariantCultureIgnoreCase);

			if (!Enabled)
				Log.Info(
					$"Updates are disabled because GoToWindow is not running from the AppData directory. Executable: {executablePath}, App Data: {appDataPath}");
		}

		public SquirrelUpdater(string updateUrl)
		{
			if (!Enabled) return;

			_updateManager = new UpdateManager(updateUrl, "GoToWindow");
		}

		public void CheckForUpdates(Action<string> callback, Action<Exception> errCallback)
		{
			if (_updateManager == null)
				return; // Disposing

			if (!Enabled)
			{
				callback(null);
				return;
			}

			var checkForUpdateTask = _updateManager.CheckForUpdate();
			checkForUpdateTask.ContinueWith(t => CheckForUpdateCallback(callback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			checkForUpdateTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CheckForUpdateCallback(Action<string> callback, UpdateInfo updateInfo)
		{
			if (_updateManager == null)
				return; // Disposing

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
			if (_updateManager == null)
				return; // Disposing
			if (!Enabled)
				throw new ApplicationException("Updates are currently disabled");

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
			if (_updateManager == null)
				return; // Disposing

			progressCallback(UpdateStatus.Installing, 0);
			Log.Info("Squirrel: Applying releases...");
			var applyReleasesTask = _updateManager.ApplyReleases(_updateInfo, progress => progressCallback(UpdateStatus.Installing, progress));
			applyReleasesTask.ContinueWith(t => ApplyReleasesCallback(progressCallback, errCallback, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
			applyReleasesTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ApplyReleasesCallback(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback, string installPath)
		{
			if (_updateManager == null)
				return; // Disposing

			progressCallback(UpdateStatus.Installing, 100);
			Log.Info("Squirrel: Creating uninstall info...");
			var createUninstallerRegistryEntryTask = _updateManager.CreateUninstallerRegistryEntry();
			createUninstallerRegistryEntryTask.ContinueWith(t => CreateUninstallerRegistryEntryCallback(progressCallback, errCallback, installPath), TaskContinuationOptions.OnlyOnRanToCompletion);
			createUninstallerRegistryEntryTask.ContinueWith(t => HandleAsyncError(errCallback, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
		}

		private void CreateUninstallerRegistryEntryCallback(Action<UpdateStatus, int> progressCallback, Action<Exception> errCallback, string installPath)
		{
			if (_updateManager == null)
				return; // Disposing

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

		private static void HandleAsyncError(Action<Exception> errCallback, Exception exc)
		{
			Log.Error("Error while trying to check for updates", exc);

			errCallback?.Invoke(exc);
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
			if (_updateManager == null)
				return; // Disposing
		   
			if (!Enabled) return;

			_updateManager.CreateShortcutsForExecutable(ExecutableFilename, ShortcutLocation.StartMenu, updateOnly, null, null);
			_updateManager.CreateShortcutsForExecutable(ExecutableFilename, ShortcutLocation.Desktop, updateOnly, null, null);
			_updateManager.CreateShortcutsForExecutable(ExecutableFilename, ShortcutLocation.Startup, updateOnly, null, null);
		}

		internal void RemoveShortcuts()
		{
			if (_updateManager == null)
				return; // Disposing
		   
			if (!Enabled) return;

			_updateManager.RemoveShortcutsForExecutable(ExecutableFilename, ShortcutLocation.StartMenu);
			_updateManager.RemoveShortcutsForExecutable(ExecutableFilename, ShortcutLocation.Desktop);
			_updateManager.RemoveShortcutsForExecutable(ExecutableFilename, ShortcutLocation.Startup);
		}
	}
}
