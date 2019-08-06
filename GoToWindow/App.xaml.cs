using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GoToWindow.Components;
using log4net;
using GoToWindow.Squirrel;
using GoToWindow.Windows;
using GoToWindow.Api;
using GoToWindow.ViewModels;

namespace GoToWindow
{
	public partial class App
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(App).Assembly, "GoToWindow");

		private IGoToWindowContext _context;
		private GoToWindowTrayIcon _menu;
		private Mutex _mutex;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var isFirstRun = false;
			if(e.Args.Any() && e.Args[0].StartsWith("--squirrel"))
			{
				var cliHandler = new SquirrelCommandLineArgumentsHandler();
				if (cliHandler.HandleSquirrelArguments(e.Args))
				{
					Log.Info("Handled Squirrel arguments. Shutting down.");
					Current.Shutdown(1);
					return;
				}

				isFirstRun = cliHandler.IsFirstRun;
			}

			if (!WaitForOtherInstancesToShutDown())
			{
				MessageBox.Show(
					"Another Go To Window instance is already running." + Environment.NewLine +
					"Exit by right-clicking the icon in the tray, and selecting 'Exit'.",
					"Go To Window",
					MessageBoxButton.OK,
					MessageBoxImage.Information
					);
				Log.Warn("Application already running. Shutting down.");
				Current.Shutdown(1);
				return;
			}

			// http://stackoverflow.com/questions/14635862/exception-occurs-while-pressing-a-button-on-touchscreen-using-a-stylus-or-a-fing
			DisableWpfTabletSupport();

			_context = new GoToWindowContext();
			
			_menu = new GoToWindowTrayIcon(_context);

			var shortcut = KeyboardShortcut.FromString(GoToWindow.Properties.Settings.Default.OpenShortcut);
			_context.EnableKeyboardHook(shortcut);

			_context.Init();

			if (shortcut.IsValid)
				Log.InfoFormat("Application started. Shortcut is '{0}' ({1})", shortcut.ToHumanReadableString(), GoToWindow.Properties.Settings.Default.OpenShortcut);
			else
				Log.WarnFormat("Application started with invalid shortcut. Shortcut is '{0}', reason: {1}", GoToWindow.Properties.Settings.Default.OpenShortcut, shortcut.InvalidReason);

			if (isFirstRun)
			{
				Log.Info("Squirrel: First run");
				Dispatcher.InvokeAsync(() =>
				{
					new FirstRunWindow(_context) {DataContext = new FirstRunViewModel()}.Show();
				});
			}
			else
			{
				_menu.ShowStartupTooltip();
			}

			SquirrelContext.AcquireUpdater().CheckForUpdates(_context.UpdateAvailable, null);
		}

		private bool WaitForOtherInstancesToShutDown()
		{
			const int msBetweenAttempts = 500;
			for(var attempt = 0; attempt < 10; attempt++)
			{
				_mutex = new Mutex(true, "GoToWindow", out bool isOnlyRunningProcessInstance);

				if (isOnlyRunningProcessInstance)
					return true;

				Log.DebugFormat("Another instance is running. Waiting for {0}ms (attempt {1})...", msBetweenAttempts, attempt + 1);
				Thread.Sleep(msBetweenAttempts);
			}

			return false;
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			SquirrelContext.Dispose();

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

			_context?.Hide(false);
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.Exception);
			MessageBox.Show("An error occured. Go To Window will shut down. Error details are available in GoToWindow.log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			Current.Shutdown((int)ExitCodes.UnhandledError);
		}

		public static void DisableWpfTabletSupport()
		{
			var devices = Tablet.TabletDevices;

			if (devices.Count <= 0) return;
			
			var inputManagerType = typeof(InputManager);

			// Call the StylusLogic method on the InputManager.Current instance.
			var stylusLogic = inputManagerType.InvokeMember("StylusLogic",
				BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
				null, InputManager.Current, null);

			if (stylusLogic == null) return;

			var stylusLogicType = stylusLogic.GetType();

			while (devices.Count > 0)
			{
				stylusLogicType.InvokeMember("OnTabletRemoved",
					BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
					null, stylusLogic, new object[] {(uint) 0});
			}
		}
	}
}
