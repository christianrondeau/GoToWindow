using GoToWindow.Api;
using System;
using System.Windows;
using System.Windows.Threading;
using GoToWindow.Windows;
using GoToWindow.ViewModels;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using log4net;

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
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessExtensions).Assembly, "GoToWindow");

        [Import(GoToWindowPluginConstants.GoToWindowPluginContractName)]
        public IGoToWindowPlugin Plugin { get; set; }

        private MainWindow _mainWindow;
        private KeyboardHook _hooks;

        public GoToWindowContext()
        {
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            var catalog = new AggregateCatalog();
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(BasicWindowsListPlugin).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog("Plugins"));
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Log.Error(compositionException);
                MessageBox.Show("An error occured while loading plug-ins. Try updating or removing plugins other than GoToWindow.Plugins.Core.dll from the Plugins directory and restart GoToWindow.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

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

                var viewModel = MainWindowViewModel.Load(Plugin);
                viewModel.Close += Hide;
                _mainWindow.DataContext = viewModel;
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

        private void Hide(object sender, EventArgs e)
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
