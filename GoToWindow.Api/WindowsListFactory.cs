using log4net;
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
        private static readonly ILog _log = LogManager.GetLogger(typeof(WindowsListFactory).Assembly, "GoToWindow");
        private const int MAX_LAST_ACTIVE_POPUP_ITERATIONS = 50;

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
                    IconHandle = GetAppIcon(hWnd)
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
            int level = MAX_LAST_ACTIVE_POPUP_ITERATIONS;
            var currentWindow = window;
            while (level-- > 0)
            {
                var lastPopUp = GetLastActivePopup(currentWindow);

                if (IsWindowVisible(lastPopUp))
                    return lastPopUp;

                if (lastPopUp == currentWindow)
                    return IntPtr.Zero;

                currentWindow = lastPopUp;
            }

            _log.Warn(string.Format("Could not find last active popup for window {0} after {1} iterations", window, MAX_LAST_ACTIVE_POPUP_ITERATIONS));
            return IntPtr.Zero;
        }

        // ReSharper disable InconsistentNaming
        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x7F;
        // ReSharper restore InconsistentNaming

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size > 4 ? GetClassLongPtr64(hWnd, nIndex) : new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public static IntPtr GetAppIcon(IntPtr hwnd)
        {
            // http://codeutopia.net/blog/2007/12/18/find-an-applications-icon-with-winapi/
            var iconHandle = SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICONSM);

            return iconHandle;
        }
    }
}
