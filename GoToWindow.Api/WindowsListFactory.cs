using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GoToWindow.Api
{
    /// <remarks>
    /// Thanks to Tommy Carlier for how to get the list of windows: http://blog.tcx.be/2006/05/getting-list-of-all-open-windows.html
    /// </remarks>
    public class WindowsListFactory
    {
        delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        public enum GetAncestorFlags
        {
            GetParent = 1,
            GetRoot = 2,
            GetRootOwner = 3
        }

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        public static WindowsList Load()
        {
            var lShellWindow = GetShellWindow();
            var windows = new List<IWindowEntry>();

            EnumWindows((hWnd, lParam) =>
            {
                string title;
                if (!EligibleForAltTab(hWnd, lShellWindow, out title))
                    return true;

                var windowTitle = GetWindowTitle(hWnd);

                if (windowTitle == null)
                    return true;

                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);

                if (processId == Process.GetCurrentProcess().Id)
                    return true;

                var process = Process.GetProcessById((int)processId);

                windows.Add(new WindowEntry
                {
                    HWnd = hWnd,
                    Title = windowTitle,
                    ProcessName = process.ProcessName,
                    Executable = process.GetExecutablePath(),
                });

                return true;
            }, 0);

            return new WindowsList(windows);
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            var lLength = GetWindowTextLength(hWnd);
            if (lLength == 0)
                return null;

            var builder = new StringBuilder(lLength);
            GetWindowText(hWnd, builder, lLength + 1);
            return builder.ToString();
        }

        private static bool EligibleForAltTab(IntPtr hWnd, IntPtr lShellWindow, out string title)
        {
            // http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does

            if (hWnd == lShellWindow)
            {
                title = null;
                return false;
            }

            var root = GetAncestor(hWnd, GetAncestorFlags.GetRootOwner);

            if (GetLastVisibleActivePopUpOfWindow(root) != hWnd)
            {
                title = null; 
                return false;
            }

            var classNameStringBuilder = new StringBuilder(256);
            var length = GetClassName(hWnd, classNameStringBuilder, classNameStringBuilder.Capacity);
            if (length == 0)
            {
                title = null;
                return false;
            }

            var className = classNameStringBuilder.ToString();

            if (className == "Shell_TrayWnd" || //Windows taskbar
                className == "DV2ControlHost" || //Windows startmenu, if open
                className == "MsgrIMEWindowClass" || //Live messenger's notifybox i think
                className == "SysShadow" || //Live messenger's shadow-hack
                className.StartsWith("WMP9MediaBarFlyout")) //WMP's "now playing" taskbar-toolbar
            {
                title = null;
                return false;
            }

            title = GetWindowTitle(hWnd);

            if (className == "Button" && title == "Start")
                return false;

            return true;
        }

        private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
        {
            while (true)
            {
                var lastPopUp = GetLastActivePopup(window);

                if (IsWindowVisible(lastPopUp))
                    return lastPopUp;

                if (lastPopUp == window)
                    return IntPtr.Zero;

                window = lastPopUp;
            }
        }
    }
}
