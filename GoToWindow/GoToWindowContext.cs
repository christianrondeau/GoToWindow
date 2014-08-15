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

        public void Hide()
        {
            if (_operationInProgress || _mainWindow == null || !_mainWindow.IsLoaded) 
                return;

			_operationInProgress = true;

            Log.Debug("Hiding Main Window.");
            _mainWindow.BeginInit();
            _mainViewModel.Empty();
            _mainWindow.EndInit();
			//TODO: Force updating of the list view, otherwise it flickers when re-opening
            Application.Current.Dispatcher.Invoke(HideWindow, DispatcherPriority.ApplicationIdle);
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

		private void LoadViewModel()
		{
			_mainWindow.BeginInit();
			_mainViewModel.Load(PluginsContainer.Plugins);
			_mainWindow.ApplyFilter();
			_mainWindow.EndInit();
			_operationInProgress = false;
		}

		private void HideWindow()
		{
			_mainWindow.Hide();
			_operationInProgress = false;
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
