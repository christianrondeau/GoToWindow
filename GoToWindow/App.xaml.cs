using GoToWindow.Api;
using GoToWindow.Commands;
using GoToWindow.Windows;
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
    public partial class App : Application
    {
        private readonly IGoToWindowContext _context = new GoToWindowContext();
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
            trayIcon.ContextMenu = CreateContextMenu();

            _context.EnableAltTabHook(GoToWindow.Properties.Settings.Default.HookAltTab);
        }

        private ContextMenu CreateContextMenu()
        {
            var contextMenu = new ContextMenu();
            var showMenu = new MenuItem { Header = "Show", Command = new OpenMainWindowCommand(_context) };
            contextMenu.Items.Add(showMenu);

            var settingsMenu = new MenuItem { Header = "Settings", Command = new ShowSettingsCommand(_context) };
            contextMenu.Items.Add(settingsMenu);

            contextMenu.Items.Add(new Separator());

            var exitMenuItem = new MenuItem { Header = "Exit", Command = new ExitCommand() };
            contextMenu.Items.Add(exitMenuItem);

            return contextMenu;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_context != null)
            {
                _context.Dispose();
            }

            if (_mutex != null)
            {
                _mutex.Dispose();
            }
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            _context.Hide();
        }
    }
}
