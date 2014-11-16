using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class InterceptAltTabTests
	{
		[TestMethod]
		public void CanInterceptAltTab()
		{
			var intercepted = false;
			var shortcut = new KeyboardShortcut
			{
				VirtualKeyCode = (int)KeyboardVirtualKeys.Tab,
				ControlVirtualKeyCode = (int)KeyboardControlKeys.Alt
			};

			using (KeyboardHook.Hook(shortcut, () => intercepted = true))
			{
				KeyboardSend.KeyDown(KeyboardSend.LAlt);
				KeyboardSend.KeyPress(KeyboardSend.Tab);
				KeyboardSend.KeyUp(KeyboardSend.LAlt);
			}

			Assert.IsTrue(intercepted, "Alt + Tab was not intercepted by InterceptAltTab");
		}
	}
}
