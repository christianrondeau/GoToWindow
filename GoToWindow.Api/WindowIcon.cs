using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public static class WindowIcon
    {
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
