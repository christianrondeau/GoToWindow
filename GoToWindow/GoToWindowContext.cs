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
		IGoToWindowPluginsContainer PluginsContainer { get; }
		void Init();
		void Show();
		void Hide();
		void EnableAltTabHook(bool enabled, int shortcutPressesBeforeOpen);
		void ShowSettings();
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

		private GoToWindowState _state = GoToWindowState.Hidden;
		private MainViewModel _mainViewModel;
		private MainWindow _mainWindow;
		private KeyboardHook _hooks;
		private IWindowEntry _mainWindowEntry;

		public IGoToWindowPluginsContainer PluginsContainer { get; private set; }

		public void Init()
		{
			WmiProcessWatcher.Start();

			PluginsContainer = GoToWindowPluginsContainer.LoadPlugins();

			_mainWindow = new MainWindow();
			_mainViewModel = new MainViewModel();
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

				SetAvailableWindowSize();
				_mainWindow.Left = SystemParameters.VirtualScreenWidth/2f - _mainViewModel.AvailableWindowWidth/2f;
				_mainWindow.Top = SystemParameters.VirtualScreenHeight / 2f - (_mainViewModel.AvailableWindowHeight + 56) / 2f;
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
		}

		private void SetAvailableWindowSize()
		{
			var width = SystemParameters.VirtualScreenWidth;
			var height = SystemParameters.VirtualScreenHeight;

			if (width > 1280)
			{
				_mainViewModel.AvailableWindowWidth = (int) (width*0.5f);
				_mainViewModel.AvailableWindowHeight = (int) (height*0.66f);
			}
			else if (width < 800)
			{
				_mainViewModel.AvailableWindowWidth = (int)(width * 0.8f);
				_mainViewModel.AvailableWindowHeight = (int)(height * 0.8f);
			}
			else
			{
				_mainViewModel.AvailableWindowWidth = 640;
				_mainViewModel.AvailableWindowHeight = (int)(height * 0.6f);
			}
		}

		public void Hide()
		{
			lock (_lock)
			{
				if (_state == GoToWindowState.Showing)
				{
					Log.Debug("Cannot Hide because Show is still in progress. Pending Hide.");
					_state = GoToWindowState.ShowingThenHide;
					return;
				}

				if (_state != GoToWindowState.Shown && _state != GoToWindowState.ShowingThenHide)
					return;

				Log.Debug("Hiding Main Window.");
				_state = GoToWindowState.Hiding;

				try
				{
					_mainWindow.BeginInit();
					_mainViewModel.Empty();
					_mainWindow.EndInit();
				}
				catch (InvalidOperationException exc)
				{
					Log.Error("Failed showing window", exc);
				}

				Application.Current.Dispatcher.InvokeAsync(HideWindowAsync, DispatcherPriority.ApplicationIdle);
			}
		}

		private async void HideWindowAsync()
		{
			await Task.Delay(100); // Ugly hack to let the windows list clear, avoiding a flicker when re-opening
			HideWindow();
		}

		public void EnableAltTabHook(bool enabled, int shortcutPressesBeforeOpen)
		{
			if (enabled)
			{
				if (_hooks != null)
				{
					_hooks.Dispose();
				}

				var shortcut = new KeyboardShortcut
				{
					VirtualKeyCode = KeyboardVirtualCodes.Tab,
					Modifier = KeyboardVirtualCodes.Modifiers.Alt,
					ShortcutPressesBeforeOpen = shortcutPressesBeforeOpen
				};

				_hooks = new KeyboardHook(shortcut, HandleAltTab);
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
			var hide = false;

			lock (_lock)
			{
				if (_state == GoToWindowState.ShowingThenHide)
				{
					hide = true;
				}
				else
				{
					_mainWindow.BeginInit();
					_mainViewModel.Load(PluginsContainer.Plugins);
					_mainWindow.DataReady();
					_mainWindow.EndInit();
					Log.Debug("View Model loaded.");
					Application.Current.Dispatcher.InvokeAsync(EnsureWindowIsForeground, DispatcherPriority.Background);
					_state = GoToWindowState.Shown;
				}
			}

			if (hide)
			{
				Log.Debug("Executing pending Hide.");
				Hide();
			}
		}

		private async void EnsureWindowIsForeground()
		{
			for (var i = 0; i < 100; i++)
			{
				await Task.Delay(10);

				if (_state == GoToWindowState.Hiding || _state == GoToWindowState.ShowingThenHide)
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
			lock (_lock)
			{
				_mainWindow.Hide();
				Log.Debug("Main window hidden.");
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
