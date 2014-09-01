using System;
using System.Windows.Controls;
using GoToWindow.Api;
using GoToWindow.Commands;
using Hardcodet.Wpf.TaskbarNotification;

namespace GoToWindow.Components
{
	public class GoToWindowTrayIcon : IDisposable
	{
		private readonly IGoToWindowContext _context;
		private TaskbarIcon _trayIcon;

		public GoToWindowTrayIcon(IGoToWindowContext context)
		{
			_context = context;

			_trayIcon = new TaskbarIcon
			{
				Icon = Properties.Resources.AppIcon,
				ToolTipText = "Go To Window",
				DoubleClickCommand = new OpenMainWindowCommand(_context),
				ContextMenu = CreateContextMenu()
			};
		}

		private ContextMenu CreateContextMenu()
		{
			var contextMenu = new ContextMenu();
			var showMenu = new MenuItem { Header = "_Show", Command = new OpenMainWindowCommand(_context) };
			contextMenu.Items.Add(showMenu);

			var settingsMenu = new MenuItem { Header = "S_ettings", Command = new ShowSettingsCommand(_context) };
			contextMenu.Items.Add(settingsMenu);

			contextMenu.Items.Add(new Separator());

			var exitMenuItem = new MenuItem { Header = "E_xit", Command = new ExitCommand() };
			contextMenu.Items.Add(exitMenuItem);

			return contextMenu;
		}

		public void ShowStartupTooltip()
		{
			var shortcutPressesBeforeOpen = Properties.Settings.Default.ShortcutPressesBeforeOpen;
			var openShortcutDescription = shortcutPressesBeforeOpen == 1
				? "Alt + Tab"
				: "Alt + Tab + Tab";

			var tooltipMessage = string.Format("Press {0} and start typing to find a window.", openShortcutDescription);

			if (!WindowsRuntimeHelper.GetHasElevatedPrivileges())
			{
				tooltipMessage += Environment.NewLine + Environment.NewLine + "NOTE: Not running with elevated privileges. Performance will be affected; Will not work in applications running as an administrator.";
			}

			_trayIcon.ShowBalloonTip(
				"Go To Window",
				tooltipMessage,
				BalloonIcon.Info);
		}

		public void Dispose()
		{
			if (_trayIcon != null)
			{
				_trayIcon.Dispose();
				_trayIcon = null;
			}
		}
	}
}