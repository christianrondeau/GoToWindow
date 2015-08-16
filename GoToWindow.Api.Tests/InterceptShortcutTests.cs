using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class InterceptShortcutTests
	{
		[TestMethod]
		public void CanInterceptAltTab()
		{
			var intercepted = false;
			var shortcut = new KeyboardShortcut((int) KeyboardControlKeys.LAlt, (int) KeyboardVirtualKeys.Tab, 1);

			using (KeyboardHook.Hook(shortcut, () => intercepted = true))
			{
				KeyboardSend.KeyDown(KeyboardSend.LAlt);
				KeyboardSend.KeyPress(KeyboardSend.Tab);
				KeyboardSend.KeyUp(KeyboardSend.LAlt);
			}

			Assert.IsTrue(intercepted, "Alt + Tab was not intercepted by InterceptAltTab");
		}

		[TestMethod]
		public void CanInterceptWinTab()
		{
			var intercepted = false;
			var shortcut = new KeyboardShortcut((int)KeyboardControlKeys.LWin, (int)KeyboardVirtualKeys.Tab, 1);

			using (KeyboardHook.Hook(shortcut, () => intercepted = true))
			{
				KeyboardSend.KeyDown(KeyboardSend.LWin);
				KeyboardSend.KeyPress(KeyboardSend.Tab);
				KeyboardSend.KeyUp(KeyboardSend.LWin);
			}

			Assert.IsTrue(intercepted, "Win + Tab was not intercepted by InterceptAltTab");
		}
	}
}
