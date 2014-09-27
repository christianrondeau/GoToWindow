using System.Linq;
using log4net;
using System.Windows;
using GoToWindow.Windows;
using System.Collections.Generic;
using System.Diagnostics;

namespace GoToWindow.Squirrel
{
	public class SquirrelCommandLineArgumentsHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SquirrelCommandLineArgumentsHandler).Assembly, "GoToWindow");

		public SquirrelCommandLineArgumentsHandler()
		{
		}

		public bool IsFirstRun { get; private set; }

		public bool HandleSquirrelArguments(string[] args)
		{
			var firstArg = args.FirstOrDefault();

			if (string.IsNullOrEmpty(firstArg))
				return false;

			switch (firstArg)
			{
				case "--squirrel-install":
					// `--squirrel-install x.y.z.m` - called when your app is installed. Exit as soon as you're finished setting up the app
					HandleSquirrelInstall(args.ElementAtOrDefault(1));
					return true;
				case "--squirrel-updated":
					// `--squirrel-updated x.y.z.m` - called when your app is updated to the given version. Exit as soon as you're finished.
					HandleSquirrelUpdated(args.ElementAtOrDefault(1));
					return true;
				case "--squirrel-uninstall":
					// `--squirrel-uninstall` - called when your app is uninstalled. Exit as soon as you're finished.
					HandleSquirrelUninstall();
					return true;
				case "--squirrel-firstrun":
					// `--squirrel-firstrun` - called after everything is set up. You should treat this like a normal app run (maybe show the "Welcome" screen)
					IsFirstRun = true;
					return false;
				case "--squirrel-firstrunafterupdate":
					// `--squirrel-firstrunsinceupdate` - called after an update was completed. You should treat this like a normal app run (maybe show the "This is new the version" screen)
					IsFirstRun = true;
					return false;
				default:
					return firstArg.StartsWith("--squirrel");
			}
		}

		private void HandleSquirrelInstall(string version)
		{
			SquirrelContext.AcquireUpdater().InstallShortcuts(false);
			Log.InfoFormat("Squirrel: Installed GoToWindow: {0}", version);
		}

		private void HandleSquirrelUpdated(string version)
		{
			SquirrelContext.AcquireUpdater().InstallShortcuts(true);
			Log.InfoFormat("Squirrel: Update GoToWindow: {0}", version);
		}

		private void HandleSquirrelUninstall()
		{
			SquirrelContext.AcquireUpdater().RemoveShortcuts();
			Log.Info("Squirrel: Uninstalling GoToWindow");
		}
	}
}
