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

        public static WindowsList Load()
        {
            var lShellWindow = GetShellWindow();
            var windows = new List<IWindowEntry>();

            EnumWindows((hWnd, lParam) =>
            {
                if (hWnd == lShellWindow)
                    return true;

                if (!IsWindowVisible(hWnd))
                    return true;

                var lLength = GetWindowTextLength(hWnd);
                if (lLength == 0)
                    return true;

                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);

                if (processId == Process.GetCurrentProcess().Id)
                    return true;

                var process = Process.GetProcessById((int)processId);

                var builder = new StringBuilder(lLength);
                GetWindowText(hWnd, builder, lLength + 1);

                windows.Add(new WindowEntry
                {
                    HWnd = hWnd,
                    Title = builder.ToString(),
                    ProcessName = process.ProcessName,
                    Executable = process.GetExecutablePath()
                });

                return true;
            }, 0);

            return new WindowsList(windows);
        }
    }
}
