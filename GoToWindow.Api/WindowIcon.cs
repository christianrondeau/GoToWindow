using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
	public static class WindowIcon
	{
		public const int PerIconTimeoutMilliseconds = 50;

		// ReSharper disable InconsistentNaming
		public const int ICON_SMALL = 0;
		public const int ICON_BIG = 1;
		public const int ICON_SMALL2 = 2;

		public const int WM_GETICON = 0x7F;

		public const int SMTO_ABORTIFHUNG = 0x0002;
		// ReSharper restore InconsistentNaming

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint  msg, uint  wParam, int lParam, uint  fuFlags, uint  uTimeout, out IntPtr lpdwResult);

		public static IntPtr GetAppIcon(IntPtr hwnd)
		{
			// http://codeutopia.net/blog/2007/12/18/find-an-applications-icon-with-winapi/
			var result = SendMessageTimeout(hwnd, WM_GETICON, ICON_BIG, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out IntPtr iconHandle);
			if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
				result = SendMessageTimeout(hwnd, WM_GETICON, ICON_SMALL, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out iconHandle);
			if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
				result = SendMessageTimeout(hwnd, WM_GETICON, ICON_SMALL2, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out iconHandle);
			if(result != IntPtr.Zero)
				return iconHandle;

			return IntPtr.Zero;
		}
	}
}
