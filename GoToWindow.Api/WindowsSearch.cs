using System.Linq;
using System.Threading;

namespace GoToWindow.Api
{
	public static class WindowsSearch
	{
		public static void Launch(string query)
		{
			KeyboardSend.KeyDown(KeyboardSend.LWin);
			if (WindowsRuntimeHelper.IsWindows8())
				KeyboardSend.KeyPress((byte) 'S');
			KeyboardSend.KeyUp(KeyboardSend.LWin);

			if (string.IsNullOrEmpty(query)) return;

			Thread.Sleep(100);

			var keysToSend = query.Trim().Select(char.ToUpper).Where(OnlyValidWindowsSearchCharacters);
			foreach (var uc in keysToSend)
			{
				KeyboardSend.KeyPress((byte) uc);
			}
		}

		private static bool OnlyValidWindowsSearchCharacters(char uc)
		{
			return uc == 0x20 || uc >= 0x30 && uc <= 0x39 || uc >= 0x41 && uc <= 0x5a;
		}
	}
}
