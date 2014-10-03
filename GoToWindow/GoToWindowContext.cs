using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GoToWindow.Api;
using GoToWindow.ViewModels;
using GoToWindow.Windows;
using log4net;

namespace GoToWindow
{
	public interface IGoToWindowContext : IDisposable
	{
		event EventHandler Showing;

		IGoToWindowPluginsContainer PluginsContainer { get; }
		void Init();
		void Show();
		void Hide();
		void EnableAltTabHook(KeyboardShortcut shortcut);
		void ShowSettings();
		void UpdateAvailable(string version);
	}

	public class GoToWindowContext : IGoToWindowContext
	{
		private enum GoToWindowState
		{
			Showing,
			ShowingThenHide,
			Shown,
			Hiding,
			Hidden
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowContext).Assembly, "GoToWindow");

		private readonly Object _lock = new Object();
		private readonly MainViewModel _mainViewModel;
		private readonly MainWindow _mainWindow;

		private GoToWindowState _state = GoToWindowState.Hidden;
		private KeyboardHook _hooks;
		private IWindowEntry _mainWindowEntry;

		public IGoToWindowPluginsContainer PluginsContainer { get; private set; }

		public event EventHandler Showing;

		public void UpdateAvailable(string version)
		{
			_mainViewModel.UpdateAvailable = !String.IsNullOrEmpty(version);
		}

		public GoToWindowContext()
		{
			_mainWindow = new MainWindow();
			_mainViewModel = new MainViewModel();
		}
		
		public void Init()
		{
			WmiProcessWatcher.Start();

			PluginsContainer = GoToWindowPluginsContainer.LoadPlugins();

			_mainWindow.DataContext = _mainViewModel;
			_mainViewModel.Close += _mainViewModel_Hide;
		}

		public void Show()
		{
			lock (_lock)
			{
				if (_state == GoToWindowState.Shown)
				{
					Log.Debug("Sending Tab Again to Main Window.");
					_mainWindow.TabAgain();
					return;
				}

				if (_state != GoToWindowState.Hidden)
					return;

				Log.Debug("Showing Main Window.");
				_state = GoToWindowState.Showing;
			}

			if (Showing != null)
				Showing(this, new EventArgs());

			SetAvailableWindowSize(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
			_mainWindow.Left = SystemParameters.PrimaryScreenWidth/2f - _mainViewModel.AvailableWindowWidth/2f;
			_mainWindow.Top = SystemParameters.PrimaryScreenHeight/2f - (_mainViewModel.AvailableWindowHeight + 56)/2f;
			_mainWindow.Show();

			if (_mainWindowEntry == null)
			{
				var interopHelper = new WindowInteropHelper(_mainWindow);
				_mainWindowEntry = WindowEntryFactory.Create(interopHelper.Handle);

				Log.DebugFormat("GoToWindow main window created with hWnd {0}", _mainWindowEntry.HWnd);
			}

			_mainWindow.SetFocus();
			_mainWindowEntry.Focus();

			Application.Current.Dispatcher.InvokeAsync(LoadViewModel, DispatcherPriority.Background);
		}

		private void SetAvailableWindowSize(double screenWidth, double screenHeight)
		{
			if (screenWidth > 1280)
			{
				_mainViewModel.AvailableWindowWidth = (int) (screenWidth*0.5f);
				_mainViewModel.AvailableWindowHeight = (int) (screenHeight*0.66f);
			}
			else if (screenWidth < 800)
			{
				_mainViewModel.AvailableWindowWidth = (int)(screenWidth * 0.8f);
				_mainViewModel.AvailableWindowHeight = (int)(screenHeight * 0.8f);
			}
			else
			{
				_mainViewModel.AvailableWindowWidth = 640;
				_mainViewModel.AvailableWindowHeight = (int)(screenHeight * 0.6f);
			}
		}

		public void Hide()
		{
			Hide(false);
		}

		public void Hide(bool hideIfPending)
		{
			lock (_lock)
			{
				if (_state == GoToWindowState.Showing)
				{
					Log.Debug("Cannot Hide because Show is still in progress. Pending Hide.");
					_state = GoToWindowState.ShowingThenHide;
					return;
				}

				if (!hideIfPending && _state == GoToWindowState.ShowingThenHide)
				{
					Log.Debug("Hide already pending.");
					return;
				}

				if (_state != GoToWindowState.Shown && _state != GoToWindowState.ShowingThenHide)
					return;

				Log.Debug("Hiding Main Window.");
				_state = GoToWindowState.Hiding;
			}

			try
			{
				_mainWindow.BeginInit();
				_mainViewModel.Empty();
				_mainWindow.EndInit();
				_mainViewModel.IsEmpty = true;
			}
			catch (InvalidOperationException exc)
			{
				Log.Error("Failed hiding window.", exc);
			}

			Application.Current.Dispatcher.InvokeAsync(HideWindow, DispatcherPriority.ApplicationIdle);
		}

		public void EnableAltTabHook(KeyboardShortcut shortcut)
		{
			if (shortcut.Enabled)
			{
				if (_hooks != null)
					_hooks.Dispose();
				/*
				var shortcut = new KeyboardShortcut
				{
					VirtualKeyCode = KeyboardVirtualCodes.Tab,
					Modifier = KeyboardVirtualCodes.Modifiers.Alt,
					ShortcutPressesBeforeOpen = shortcutPressesBeforeOpen
				};
				 * */

				_hooks = KeyboardHook.Hook(shortcut, HandleAltTab);
			}
			else if (_hooks != null)
			{
				_hooks.Dispose();
				_hooks = null;
			}
		}

		public void ShowSettings()
		{
			if (Application.Current.Windows.OfType<SettingsWindow>().Any())
				return;

			var settingswindow = new SettingsWindow
			{
				DataContext = new SettingsViewModel(this)
			};

			settingswindow.ShowDialog();
		}

		private void LoadViewModel()
		{
			if (_state == GoToWindowState.ShowingThenHide)
			{
				Log.Debug("Executing pending Hide (before loading windows).");
				Hide(true);
				return;
			}

			Log.Debug("View Model loading...");
			_mainWindow.BeginInit();
			_mainViewModel.Load(PluginsContainer.Plugins);
			_mainWindow.DataReady();
			_mainWindow.EndInit();
			Log.Debug("View Model loaded.");

			if (_state == GoToWindowState.ShowingThenHide)
			{
				Log.Debug("Executing pending Hide (after loading windows).");
				Hide(true);
				return;
			}

			_state = GoToWindowState.Shown;
			Application.Current.Dispatcher.InvokeAsync(EnsureWindowIsForeground, DispatcherPriority.Background);
		}

		private async void EnsureWindowIsForeground()
		{
			for (var i = 0; i < 100; i++)
			{
				await Task.Delay(10);

				if (_state != GoToWindowState.Shown)
				{
					Log.Debug("Ensuring foreground: Hide is already in progress. Stop watching.");
					return;
				}

				if (HideWindowIfNotForeground())
					return;
			}
		}

		private bool HideWindowIfNotForeground()
		{
			if (_mainWindowEntry.IsForeground())
				return false;

#if(DEBUG)
			var foregroundWindow = WindowEntryFactory.Create(WindowToForeground.GetForegroundWindow());
			Log.DebugFormat("Window does not have focus when initialization is complete. Current foreground window is {0} (Process '{1}')", foregroundWindow.HWnd, foregroundWindow.ProcessName);
#endif

			Hide();
			return true;
		}

		private void HideWindow()
		{
			_mainWindow.Hide();
			Log.Debug("Main window hidden.");

			lock (_lock)
			{
				_state = GoToWindowState.Hidden;
			}
		}

		private void HandleAltTab()
		{
			Application.Current.Dispatcher.InvokeAsync(Show, DispatcherPriority.Normal);
		}

		private void _mainViewModel_Hide(object sender, EventArgs e)
		{
			Hide();
		}

		public void Dispose()
		{
			WmiProcessWatcher.Stop();

			if (_hooks != null)
			{
				_hooks.Dispose();
				_hooks = null;
			}
		}
	}
}
