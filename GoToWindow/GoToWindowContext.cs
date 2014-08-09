using System.Windows.Input;
using GoToWindow.Api;
using System;
using System.Windows;
using System.Windows.Threading;
using GoToWindow.Windows;

namespace GoToWindow
{
    public interface IGoToWindowContext : IDisposable
    {
        void Show();
        void Hide();
        void EnableAltTabHook(bool enabled);
    }

    public class GoToWindowContext : IGoToWindowContext
    {
        delegate void ActionDelegate();

        private MainWindow _mainWindow;
        private HotKey _hooks;

        public void Show()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
            {
                _mainWindow.TabAgain();
            }
            else
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closing += MainWindow_Closing;
                _mainWindow.Show();
            }
        }

        void MainWindow_Closing(object sender, EventArgs e)
        {
            _mainWindow = null;
        }

        public void Hide()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
                _mainWindow.Close();
        }

        public void EnableAltTabHook(bool enabled)
        {
            if (_hooks == null && enabled)
            {
                _hooks = new HotKey(Key.Y, KeyModifier.Win, HandleAltTab);
            }
            else if (_hooks != null && !enabled)
            {
                _hooks.Dispose();
                _hooks = null;
            }
        }

        private void HandleAltTab(HotKey hotKey)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new ActionDelegate(Show),
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
