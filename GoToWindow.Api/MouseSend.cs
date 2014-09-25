using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
	public static class MouseSend
	{
        // ReSharper disable InconsistentNaming
		private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
		private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        // ReSharper restore InconsistentNaming

		[DllImport("user32.dll")]
		private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);

		[DllImport("user32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		public static void Click(int x, int y)
		{
			SetCursorPos(x, y);
			mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
			mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
		}
	}
}
