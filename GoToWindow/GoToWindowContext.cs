using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GoToWindow.Api;
using GoToWindow.ViewModels;
using GoToWindow.Windows;
using log4net;
using System.Threading;
using System.Threading.Tasks;

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
		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowContext).Assembly, "GoToWindow");

		private Object _lock = new Object();
	    private bool _showInProgress;
		private bool _hideInProgress;
		private bool _hidePending;
		private MainViewModel _mainViewModel;
        private MainWindow _mainWindow;
        private KeyboardHook _hooks;
		private IWindowEntry _mainWindowEntry;

		public IGoToWindowPluginsContainer PluginsContainer { get; private set; }

	    public void Init()
        {
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
				if (_showInProgress)
					return;

				if (_mainWindow.Visibility == Visibility.Visible && _mainWindow.IsLoaded)
				{
					Log.Debug("Sending Tab Again to Main Window.");

					_mainWindow.TabAgain();
				}
				else
				{
					Log.Debug("Showing Main Window.");
					_showInProgress = true;

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
        }

		private void ForceWindowFocus()
		{
			if (!_mainWindowEntry.IsForeground())
			{
				_mainWindow.SetFocus();
				Log.Debug("Window does not have focus when shown. Forcing focus.");
				_mainWindowEntry.Focus();
			}
		}

        public void Hide()
        {
			lock (_lock)
			{
				if (_hideInProgress || _hidePending)
					return;

				if (_showInProgress || _mainWindow == null || !_mainWindow.IsLoaded)
				{
					if (!_hidePending)
					{
						Log.Debug("Cannot Hide because Show is still in progress. Pending Hide.");
						_hidePending = true;
						return;
					}
				}

				_hideInProgress = true;

				Log.Debug("Hiding Main Window.");
				try
				{
					_mainWindow.BeginInit();
					_mainViewModel.Empty();
					_mainWindow.EndInit();
				}
				catch (InvalidOperationException exc)
				{
					Log.Warn("Failed showing window", exc);
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
				if(_hooks != null)
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
			else if (_hooks != null && !enabled)
			{
				_hooks.Dispose();
				_hooks = null;
			}
		}

		public void ShowSettings()
		{
			if (Application.Current.Windows.OfType<SettingsWindow>().Any())
				return;

			var settingswindow = new SettingsWindow();
			settingswindow.DataContext = new SettingsViewModel(this);
			settingswindow.ShowDialog();
		}

		private void LoadViewModel()
		{
			lock (_lock)
			{
				_mainWindow.BeginInit();
				_mainViewModel.Load(PluginsContainer.Plugins);
				_mainWindow.DataReady();
				_mainWindow.EndInit();
				Log.Debug("View Model loaded.");
				Application.Current.Dispatcher.InvokeAsync(EnsureWindowIsForeground, DispatcherPriority.Background);
				_showInProgress = false;
			}

			RunPendingHide();
		}

		private async void EnsureWindowIsForeground()
		{
			for (int i = 0; i < 100; i++)
			{
				await Task.Delay(10);

				if (_hideInProgress || _hidePending)
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

		private void RunPendingHide()
		{
			bool shouldRun = false;

			lock(_lock)
			{
				if (!_hidePending)
					return;

				Log.Debug("Executing pending Hide.");
				_hidePending = false;
				shouldRun = true;
			}

			if (shouldRun)
				Hide();
		}

		private void HideWindow()
		{
			lock (_lock)
			{
				_mainWindow.Hide();
				Log.Debug("Main window hidden.");
				_hideInProgress = false;
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
			if (_hooks != null)
			{
				_hooks.Dispose();
				_hooks = null;
			}
		}
	}
}
