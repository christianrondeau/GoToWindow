using System;
using System.Security.Principal;

namespace GoToWindow.Api
{
	public class WindowsRuntimeHelper
	{
		private static readonly Version Win8Version = new Version(6, 2, 9200, 0);

		public static bool IsWindows8()
		{
			return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= Win8Version;
		}

        public static bool GetHasElevatedPrivileges()
        {
            if (!IsWindows8()) return true;

            var identity = WindowsIdentity.GetCurrent();

	        var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(0x200);
        }
	}
}
