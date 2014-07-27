using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GoToWindow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IGoToWindowContext _context = new GoToWindowContext();
        private InterceptAltTab _keyHandler;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
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

        private void HandleAltTab()
        {
            new OpenMainWindowCommand(_context).Execute(null);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_keyHandler != null)
            {
                _keyHandler.Dispose();
                _keyHandler = null;
            }
        }
    }
}
