using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public class WindowControlMenuDisabler
    {
        // ReSharper disable InconsistentNaming
        private const int GWL_STYLE = -16; //WPF's Message code for Title Bar's Style 
        private const int WS_SYSMENU = 0x80000; //WPF's Message code for System Menu
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void DisableControlMenu(IntPtr hWnd)
        {
            SetWindowLong(hWnd, GWL_STYLE, GetWindowLong(hWnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}