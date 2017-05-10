using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using GoToWindow.Api;
using GoToWindow.Properties;
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
		void Hide(bool requested);
		void EnableKeyboardHook(KeyboardShortcut shortcut);
		void ShowSettings();
		void UpdateAvailable(string version);
	}

	public class GoToWindowContext : IGoToWindowContext
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowContext).Assembly, "GoToWindow");

		private readonly object _lock = new object();
		private MainWindow _mainWindow;
		private IWindowEntry _mainWindowEntry;
		private MainViewModel _mainViewModel;
		private KeyboardHook _hooks;
		private bool _isClosing;

		public IGoToWindowPluginsContainer PluginsContainer { get; private set; }

		public event EventHandler Showing;

		public void UpdateAvailable(string version)
		{
			_mainViewModel.UpdateAvailable = !string.IsNullOrEmpty(version);
		}

		public void Init()
		{
			WmiProcessWatcher.Start();

			PluginsContainer = GoToWindowPluginsContainer.LoadPlugins();
		}

		public void Show()
		{
			lock (_lock)
			{
				if (_mainWindow != null)
				{
					Log.Debug("Sending Tab Again to Main Window.");
					_mainWindow.ShortcutAgain();
					return;
				}

				_mainViewModel = new MainViewModel();
				_mainWindow = new MainWindow { DataContext = _mainViewModel };
				_mainWindow.Closing += _mainWindow_Closing;
				_mainViewModel.Close += _mainViewModel_Close;

				Showing?.Invoke(this, new EventArgs());

				SetAvailableWindowSize(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
				_mainWindow.Left = SystemParameters.PrimaryScreenWidth / 2f - _mainViewModel.AvailableWindowWidth / 2f;
				_mainWindow.Top = SystemParameters.PrimaryScreenHeight / 2f - (_mainViewModel.AvailableWindowHeight + 56) / 2f;
				_mainWindow.Show();

				var interopHelper = new WindowInteropHelper(_mainWindow);
				_mainWindowEntry = WindowEntryFactory.Create(interopHelper.Handle);

				Log.DebugFormat("GoToWindow main window created with hWnd {0}", _mainWindowEntry.HWnd);

				_mainWindow.SetFocus();
				_mainWindowEntry.Focus();

				Application.Current.Dispatcher.InvokeAsync(LoadViewModel, DispatcherPriority.Input);
			}
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

		public void Hide(bool requested)
		{
			Hide(false, requested, _mainWindow);
		}

		public void Hide(bool hideIfPending, bool requested, MainWindow target)
		{
			if (!requested && Settings.Default.KeepOpenOnLostFocus)
			{
				Log.Debug("Ignoring hide on focus out because KeepOpenOnLostFocus is active.");
				return;
			}

			lock (_lock)
			{
				if (_mainWindow == null || !ReferenceEquals(_mainWindow, target))
				{
					Log.Debug("Ignore hide request because window is already hidden.");
					return;
				}

				try
				{
					_mainWindow.Close();
					Log.Debug("Window Hidden");
				}
				catch (InvalidOperationException exc)
				{
					Log.Warn("Window is still closing", exc);
				}
				catch (Exception exc)
				{
					Log.Error("Failed hiding window.", exc);
				}

				_mainWindow = null;
				_mainViewModel = null;
				_mainWindowEntry = null;
			}
		}

		public void EnableKeyboardHook(KeyboardShortcut shortcut)
		{
			if (shortcut.Enabled)
			{
				_hooks?.Dispose();
				_hooks = KeyboardHook.Hook(shortcut, HandleShortcut);
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
			lock (_lock)
			{
				if (_mainWindow == null)
				{
					Log.Debug("Cancelling LoadViewModel because Window was closed.");
					return;
				}

				try
				{
					Log.Debug("View Model loading...");
					_mainWindow.BeginInit();
					_mainViewModel.Load(PluginsContainer.Plugins);
					_mainWindow.DataReady();
					_mainWindow.EndInit();
					Log.Debug("View Model loaded.");
				}
				catch (Exception exc)
				{
					Log.Error("Failed LoadViewModel.", exc);
				}

				Application.Current.Dispatcher.InvokeAsync(EnsureWindowIsForeground, DispatcherPriority.Background);
			}
		}

		private async void EnsureWindowIsForeground()
		{
			for (var i = 0; i < 100; i++)
			{
				await Task.Delay(10);

				if (_mainWindow == null) return;
				if (HideWindowIfNotForeground()) return;
			}
		}

		private bool HideWindowIfNotForeground()
		{
			if (_mainWindowEntry?.IsForeground() ?? false)
				return false;

#if(DEBUG)
			var foregroundWindow = WindowEntryFactory.Create(WindowToForeground.GetForegroundWindow());
			Log.DebugFormat("Window does not have focus when initialization is complete. Current foreground window is {0} (Process '{1}')", foregroundWindow.HWnd, foregroundWindow.ProcessName);
#endif

			Hide(false);
			return true;
		}

		private void HandleShortcut()
		{
			Application.Current.Dispatcher.InvokeAsync(Show, DispatcherPriority.Normal);
		}

		private void _mainWindow_Closing(object sender, CancelEventArgs e)
		{
			_isClosing = true;
		}

		private void _mainViewModel_Close(object sender, CloseEventArgs e)
		{
			Application.Current.Dispatcher.InvokeAsync(() => Hide(false, e.Requested, sender as MainWindow), DispatcherPriority.Input);
		}

		public void Dispose()
		{
			WmiProcessWatcher.Stop();

			_hooks?.Dispose();
		}
	}
}
