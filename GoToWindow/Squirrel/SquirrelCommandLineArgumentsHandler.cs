using System.Linq;
using log4net;
using System.Windows;

namespace GoToWindow.Squirrel
{
	public class SquirrelCommandLineArgumentsHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(SquirrelCommandLineArgumentsHandler).Assembly, "GoToWindow");

		public bool HandleSquirrelArguments(string[] args)
		{
			switch (args.FirstOrDefault())
			{
				case "--squirrel-install":
					// `--squirrel-install x.y.z.m` - called when your app is installed. Exit as soon as you're finished setting up the app
					HandleSquirrelInstall(args.ElementAtOrDefault(1));
					return true;
				case "--squirrel-firstrun":
					// `--squirrel-firstrun` - called after everything is set up. You should treat this like a normal app run (maybe show the "Welcome" screen)
					HandleSquirrelFirstRun();
					return false;
				case "--squirrel-firstrunafterupdate":
					// `--squirrel-firstrunsinceupdate` - called after an update was completed. You should treat this like a normal app run (maybe show the "This is new the version" screen)
					HandleSquirrelFirstRunAfterUpdate();
					return false;
				case "--squirrel-updated":
					// `--squirrel-updated x.y.z.m` - called when your app is updated to the given version. Exit as soon as you're finished.
					HandleSquirrelUpdated(args.ElementAtOrDefault(1));
					return true;
				case "--squirrel-uninstall":
					// `--squirrel-uninstall` - called when your app is uninstalled. Exit as soon as you're finished.
					HandleSquirrelUninstall();
					return true;
				default:
					return false;
			}
		}

		private void HandleSquirrelInstall(string version)
		{
			//TODO: Close other instances
			Log.InfoFormat("Squirrel: Installed GoToWindow: {0}", version);
		}

		private void HandleSquirrelFirstRun()
		{
			Log.Info("Squirrel: First run");
			MessageBox.Show("Go To Window successfully installed!", "Go To Window Installed!", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void HandleSquirrelFirstRunAfterUpdate()
		{
			Log.Info("Squirrel: First run after update");
			MessageBox.Show("Go To Window successfully updated!", "Go To Window Updated!", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void HandleSquirrelUpdated(string version)
		{
			//TODO: Close other instances
			Log.InfoFormat("Squirrel: Update GoToWindow: {0}", version);
		}

		private void HandleSquirrelUninstall()
		{
			//TODO: Close other instances
			Log.Info("Squirrel: Uninstalling GoToWindow");
		}
	}
}
