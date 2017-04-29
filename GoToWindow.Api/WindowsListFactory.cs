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
	/// Thanks to taby for window eligibility: http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
	/// Thanks to Hans Passant & Tim Beaudet for Windows 10 apps process name: http://stackoverflow.com/a/32513438/154480
    /// </remarks>
    public static class WindowsListFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsListFactory).Assembly, "GoToWindow");
        private const int MaxLastActivePopupIterations = 50;

	    private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        public enum GetAncestorFlags
        {
            GetParent = 1,
            GetRoot = 2,
            GetRootOwner = 3
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetLastActivePopup(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static WindowsList Load()
        {
            var lShellWindow = GetShellWindow();
            var windows = new List<IWindowEntry>();
            var currentProcessId = Process.GetCurrentProcess().Id;

	        EnumWindows((hWnd, lParam) =>
	        {
		        InspectPotentialWindow(hWnd, lShellWindow, currentProcessId, windows);
		        return true;
	        }, 0);

            return new WindowsList(windows);
        }

	    private static void InspectPotentialWindow(IntPtr hWnd, IntPtr lShellWindow, int currentProcessId, ICollection<IWindowEntry> windows)
	    {
		    if (!HWndEligibleForActivation(hWnd, lShellWindow))
			    return;

		    var className = GetClassName(hWnd);

		    if (className == "ApplicationFrameWindow")
			    InspectWindows10AppWindow(hWnd, windows, className);
		    else
			    InspectNormalWindow(hWnd, currentProcessId, windows, className);
	    }

		private static void InspectNormalWindow(IntPtr hWnd, int currentProcessId, ICollection<IWindowEntry> windows, string className)
		{
			if (!ClassEligibleForActivation(className))
				return;

			var window = WindowEntryFactory.Create(hWnd);

			if (IsKnownException(window))
				return;

			if (window.ProcessId == currentProcessId || window.Title == null)
				return;

			UpdateProcessName(window);
			windows.Add(window);
			LogDebugWindow("- Found normal window: ", window, className);
		}

	    private static void InspectWindows10AppWindow(IntPtr hWnd, ICollection<IWindowEntry> windows, string className)
	    {
		    Log.Debug("- Found Window 10 App");

		    var foundChildren = false;
			GetWindowThreadProcessId(hWnd, out uint processId);

			EnumChildWindows(hWnd, (childHWnd, lparam) =>
		    {
				GetWindowThreadProcessId(childHWnd, out uint childProcessId);
				Log.Debug("  - Checking process: " + childProcessId);
			    if (processId != childProcessId)
			    {
				    var childClassName = GetClassName(hWnd);

				    var window = WindowEntryFactory.Create(childHWnd, childProcessId);
				    if (window.Title != null)
				    {
						UpdateProcessName(window);

					    if (IsKnownWindows10Exception(window)) return true;

						//TODO: Windows 10 App Icons
						// 1. Get the window.ProcessFileName
						// 2. Look in the folder for AppManifest.xml
						// 3. Look for Package/Properties/Logo
						// 4. Load that file (should be a PNG)

					    windows.Add(window);
					    foundChildren = true;
					    LogDebugWindow("    - Found window: ", window, childClassName);
				    }
			    }
			    return true;
		    }, IntPtr.Zero);

		    if (!foundChildren)
		    {
				var window = WindowEntryFactory.Create(hWnd, processId);
			    if (window.Title != null)
			    {
				    window.ProcessName = "Windows 10 App";
				    windows.Add(window);
				    LogDebugWindow("    - No windows found: ", window, className);
			    }
		    }
	    }

	    private static bool IsKnownWindows10Exception(WindowEntry window)
	    {
		    if (window.ProcessName == "MicrosoftEdge")
			    return true;

		    if (window.ProcessName == "MicrosoftEdgeCP")
		    {
			    if (window.Title == "CoreInput")
				    return true;

				if (window.Title == "about:tabs")
					return true;
		    }

		    return false;
	    }

	    [Conditional("DEBUG")]
	    private static void LogDebugWindow(string message, WindowEntry window, string className)
	    {
			Log.Debug(message + window + ", Class: " + className);
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

	    private static void UpdateProcessName(IWindowEntry window)
	    {
		    window.ProcessName = WmiProcessWatcher.GetProcessName(window.ProcessId, () =>
		    {
			    using (var process = Process.GetProcessById((int) window.ProcessId))
			    {
					//TODO: Windows 10 App Icons
					/*
				    try
				    {
						window.ProcessFileName = process.MainModule.FileName;
				    }
				    catch (Exception ex)
				    {
						Log.Warn("Could not get the executable name of the process " + window, ex);
				    }
					 * */
				    return process.ProcessName;
			    }
		    });
	    }

	    private static bool IsKnownException(IWindowEntry window)
	    {
		    if (window.ProcessName == "Fiddler" && window.Title == "SSFiddlerMsgWin")
			    return true;

		    return false;
	    }

	    private static readonly string[] WindowsClassNamesToSkip =
		{
			"Shell_TrayWnd", // Task Bar
			"DV2ControlHost", // Start Menu
			"MsgrIMEWindowClass", // Messenger
			"SysShadow", // Messenger
			"Button", // UI component, e.g. Start Menu button
			"Windows.UI.Core.CoreWindow", // Windows 10 Store Apps
			"Frame Alternate Owner", // Edge
			"MultitaskingViewFrame", // The original Win + Tab view
		};

        private static bool HWndEligibleForActivation(IntPtr hWnd, IntPtr lShellWindow)
        {
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

            Log.Warn( $"Could not find last active popup for window {window} after {MaxLastActivePopupIterations} iterations");
            return IntPtr.Zero;
        }
    }
}
