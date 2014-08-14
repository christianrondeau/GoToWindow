using System;

namespace GoToWindow.Api
{
	public class WindowsVersion
	{
		private static readonly Version Win8Version = new Version(6, 2, 9200, 0);

		public static bool IsWindows8()
		{
			return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= Win8Version;
		}
	}
}
