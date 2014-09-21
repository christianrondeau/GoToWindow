using System;
using System.Linq;
using System.Threading;
using System.Windows;
using GoToWindow.Components;
using log4net;
using GoToWindow.Squirrel;

namespace GoToWindow
{
	public partial class App : Application
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(App).Assembly, "GoToWindow");

		private IGoToWindowContext _context;
		private GoToWindowTrayIcon _menu;
		private Mutex _mutex;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if(e.Args != null && e.Args.Any() && e.Args[0].StartsWith("--squirrel"))
			{
				var cliHandler = new CommandLineArgumentsHandler();
				if (cliHandler.HandleSquirrelArguments(e.Args))
				{
					Log.Info("Handled Squirrel arguments. Shutting down.");
					Current.Shutdown(1);
					return;
				}
			}
		
			bool mutexCreated;
			_mutex = new Mutex(true, "GoToWindow", out mutexCreated);
			if (!mutexCreated)
			{
				Log.Warn("Application already running. Shutting down.");
				Current.Shutdown(1);
				return;
			}

			_context = new GoToWindowContext();
			
			_menu = new GoToWindowTrayIcon(_context);

			_context.EnableAltTabHook(
				GoToWindow.Properties.Settings.Default.HookAltTab,
				GoToWindow.Properties.Settings.Default.ShortcutPressesBeforeOpen
				);

			_context.Init();

			Log.Info("Application started.");

			_menu.ShowStartupTooltip();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (_menu != null)
			{
				_menu.Dispose();
				_menu = null;
			}

			if (_context != null)
			{
				_context.Dispose();
				_context = null;
			}

			if (_mutex != null)
			{
				_mutex.Dispose();
				_mutex = null;
			}

			Log.Info("Application exited.");
		}

		private void Application_Activated(object sender, EventArgs e)
		{
			Log.Debug("Application activated.");
		}

		private void Application_Deactivated(object sender, EventArgs e)
		{
			Log.Debug("Application deactivated.");

			_context.Hide();
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.Exception);
			MessageBox.Show("An error occured. Go To Window will shut down. Error details are available in GoToWindow.log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			Application.Current.Shutdown((int)ExitCodes.UnhandledError);
		}
	}
}
