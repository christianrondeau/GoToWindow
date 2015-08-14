using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace GoToWindow.Api
{
    /// <remarks>
    /// Thanks to Tommy Carlier for how to get the list of windows: http://blog.tcx.be/2006/05/getting-list-of-all-open-windows.html
    /// </remarks>
    public static class WindowsListFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsListFactory).Assembly, "GoToWindow");
        private const int MaxLastActivePopupIterations = 50;

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
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

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
            var currentProcessId = Process.GetCurrentProcess().Id;

            EnumWindows((hWnd, lParam) =>
            {
                if (!HWndEligibleForActivation(hWnd, lShellWindow))
                    return true;

	            var className = GetClassName(hWnd);

	            if (!ClassEligibleForActivation(className))
					return true;

                var window = WindowEntryFactory.Create(hWnd);

                if (window == null || window.ProcessId == currentProcessId || window.Title == null)
                    return true;

	            window.ProcessName = GetProcessName(window);

#if(DEBUG)
				Log.DebugFormat("Found Window: {0} {1} Class: '{2}' Title: '{3}'", window.ProcessId, window.ProcessName, className, window.Title);
#endif

	            if (IsKnownException(window))
		            return true;

	            windows.Add(window);

                return true;
            }, 0);

            return new WindowsList(windows);
        }

	    private static bool ClassEligibleForActivation(string className)
	    {
		    if (Array.IndexOf(WindowsClassNamesToSkip, className) > -1)
			    return false;

		    if (className.StartsWith("WMP9MediaBarFlyout")) //WMP's "now playing" taskbar-toolbar
			    return false;

		    return true;
	    }

	    private static string GetClassName(IntPtr hWnd)
	    {
		    var classNameStringBuilder = new StringBuilder(256);
		    var length = GetClassName(hWnd, classNameStringBuilder, classNameStringBuilder.Capacity);
		    return length == 0 ? null : classNameStringBuilder.ToString();
	    }

	    private static string GetProcessName(IWindowEntry window)
	    {
		    var processName = WmiProcessWatcher.GetProcessName(window.ProcessId, () => window.ProcessName);

		    if (processName == null)
		    {
			    using (var process = Process.GetProcessById((int) window.ProcessId))
			    {
				    processName = process.ProcessName;
			    }
			}

			if ("WWAHost".Equals(processName, StringComparison.OrdinalIgnoreCase))
				return WindowsStoreApp.GetModelId(window.ProcessId);

			if ("ApplicationFrameHost".Equals(processName, StringComparison.OrdinalIgnoreCase))
				return WindowsStoreApp.GetPackageName(window.ProcessId); //return "Store App";

		    return processName;
	    }

	    private static bool IsKnownException(IWindowEntry window)
	    {
		    if (window.ProcessName == "Fiddler" && window.Title == "SSFiddlerMsgWin")
			    return true;

		    return false;
	    }

	    private static readonly string[] WindowsClassNamesToSkip =
		{
			"Shell_TrayWnd",
			"DV2ControlHost",
			"MsgrIMEWindowClass",
			"SysShadow",
			"Button",
			"Windows.UI.Core.CoreWindow", // Windows 10 Store Apps when minimized
		};

        private static bool HWndEligibleForActivation(IntPtr hWnd, IntPtr lShellWindow)
        {
            // http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does

            if (hWnd == lShellWindow)
                return false;

            var root = GetAncestor(hWnd, GetAncestorFlags.GetRootOwner);

            if (GetLastVisibleActivePopUpOfWindow(root) != hWnd)
                return false;

            return true;
        }

        private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr window)
        {
            var level = MaxLastActivePopupIterations;
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

            Log.Warn(string.Format("Could not find last active popup for window {0} after {1} iterations", window, MaxLastActivePopupIterations));
            return IntPtr.Zero;
        }
    }
}
