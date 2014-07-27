using GoToWindow.Api;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GoToWindow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IGoToWindowContext _context = new GoToWindowContext();
        private InterceptAltTab _keyHandler;
        private Mutex _mutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool mutexCreated;
            _mutex = new Mutex(true, "GoToWindow", out mutexCreated);
            if (!mutexCreated)
            {
                Application.Current.Shutdown(1);
                return;
            }

            TaskbarIcon trayIcon = new TaskbarIcon();
            trayIcon.Icon = GoToWindow.Properties.Resources.AppIcon;
            trayIcon.ToolTipText = "Go To Window";
            trayIcon.DoubleClickCommand = new OpenMainWindowCommand(_context);
            
            var contextMenu = new ContextMenu();
            var showMenu = new MenuItem { Header = "Show", Command = new OpenMainWindowCommand(_context) };
            contextMenu.Items.Add(showMenu);
            var exitMenuItem = new MenuItem { Header = "Exit", Command = new ExitCommand() };
            contextMenu.Items.Add(exitMenuItem);

            trayIcon.ContextMenu = contextMenu;

            _keyHandler = new InterceptAltTab(HandleAltTab);
        }

        delegate void CommandDelegate(object parameters);

        private void HandleAltTab()
        {
            var cmd = new OpenMainWindowCommand(_context);
            Application.Current.Dispatcher.BeginInvoke(
                new CommandDelegate(cmd.Execute),
                DispatcherPriority.Normal,
                new Object[] { null });
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }

            if (_keyHandler != null)
            {
                _keyHandler.Dispose();
                _keyHandler = null;
            }
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            _context.Hide();
        }
    }
}
