using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GoToWindow.Api
{
	public static class WindowIcon
	{
		// ReSharper disable InconsistentNaming
		public const int ICON_SMALL = 0;
		public const int ICON_BIG = 1;
		public const int ICON_SMALL2 = 2;

		public const int WM_GETICON = 0x7F;
		// ReSharper restore InconsistentNaming

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

		public static IntPtr GetAppIcon(IntPtr hwnd)
		{
			var task = Task.Run(() => GetAppIconCore(hwnd));
			return task.Wait(TimeSpan.FromMilliseconds(100)) ? task.Result : hwnd;
		}

		private static IntPtr GetAppIconCore(IntPtr hwnd)
		{
			// http://codeutopia.net/blog/2007/12/18/find-an-applications-icon-with-winapi/
			var iconHandle = SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
			if (iconHandle == IntPtr.Zero)
				iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
			if (iconHandle == IntPtr.Zero)
				iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);

			return iconHandle;
		}
	}
}
