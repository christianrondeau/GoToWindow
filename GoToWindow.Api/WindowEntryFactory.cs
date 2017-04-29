using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GoToWindow.Api
{
	public static class WindowEntryFactory
	{
		[DllImport("user32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

		public static WindowEntry Create(IntPtr hWnd)
		{
			GetWindowThreadProcessId(hWnd, out uint processId);

			return Create(hWnd, processId);
		}

		public static WindowEntry Create(IntPtr hWnd, uint processId)
		{
			var windowTitle = GetWindowTitle(hWnd);

		    var iconHandle = WindowIcon.GetAppIcon(hWnd);
            var isVisible = !IsIconic(hWnd);

		    return new WindowEntry
			{
				HWnd = hWnd,
				Title = windowTitle,
				ProcessId = processId,
				IconHandle = iconHandle,
                IsVisible = isVisible
			};
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
	}
}
