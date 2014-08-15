using System;
using System.Linq;
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
		void EnableAltTabHook(bool enabled);
		void ShowSettings();
	}

	public class GoToWindowContext : IGoToWindowContext
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowContext).Assembly, "GoToWindow");

	    private bool _operationInProgress;
		private MainViewModel _mainViewModel;
        private MainWindow _mainWindow;
        private KeyboardHook _hooks;

		public IGoToWindowPluginsContainer PluginsContainer { get; private set; }

		public GoToWindowContext()
		{
			PluginsContainer = GoToWindowPluginsContainer.LoadPlugins();
		}

        public void Init()
        {
            _mainWindow = new MainWindow();
			_mainViewModel = new MainViewModel();
			//var previousWidth = _mainWindow.Width = 0;
			//var previousHeight = _mainWindow.Height = 0;
			_mainWindow.DataContext = _mainViewModel;
			_mainViewModel.Close += _mainViewModel_Hide;

			//Show();
        }

        public void Show()
        {
            if (_operationInProgress)
                return;

            if (_mainWindow.Visibility == Visibility.Visible && _mainWindow.IsLoaded)
            {
                Log.Debug("Sending Tab Again to Main Window.");

				_mainWindow.TabAgain();
			}
			else
			{
				Log.Debug("Showing Main Window.");
			    _operationInProgress = true;

                _mainWindow.Show();

				var interopHelper = new WindowInteropHelper(_mainWindow);
				var thisEntry = WindowEntryFactory.Create(interopHelper.Handle);

				_mainWindow.SetFocus();

				if (!thisEntry.HasFocus())
				{
					Log.Debug("Window does not have focus when shown. Forcing focus.");
					thisEntry.Focus();
				}

				Application.Current.Dispatcher.InvokeAsync(LoadViewModel, DispatcherPriority.Background);
            }
        }

		private void LoadViewModel()
		{
			_mainWindow.BeginInit();
			_mainViewModel.Load(PluginsContainer.Plugins);
			_mainWindow.ApplyFilter();
			_mainWindow.EndInit();
		    _operationInProgress = false;
		}

        public void Hide()
        {
            if (_operationInProgress || _mainWindow == null || !_mainWindow.IsLoaded) 
                return;

			_operationInProgress = true;

            Log.Debug("Hiding Main Window.");
            _mainWindow.BeginInit();
            _mainViewModel.Empty();
            _mainWindow.EndInit();
            Application.Current.Dispatcher.InvokeAsync(HideComplete, DispatcherPriority.Background);
        }

		private void HideComplete()
		{
			_mainWindow.Hide();
			_operationInProgress = false;
		}

		private void _mainViewModel_Hide(object sender, EventArgs e)
		{
			Hide();
		}

		public void EnableAltTabHook(bool enabled)
		{
			if (_hooks == null && enabled)
			{
				_hooks = new KeyboardHook(HandleAltTab);
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

		private void HandleAltTab()
		{
			Application.Current.Dispatcher.BeginInvoke(
				new Action(Show),
				DispatcherPriority.Normal,
				null);
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
