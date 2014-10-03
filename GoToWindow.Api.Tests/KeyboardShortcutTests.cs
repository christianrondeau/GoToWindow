using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class KeyboardShortcutTests
	{
		[TestMethod]
		public void ToStringContainsAllKeys()
		{
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = 0xA4,
				VirtualKeyCode = 0x09,
				ShortcutPressesBeforeOpen = 2
			};
			Assert.AreEqual("A4+09:2", shortcut.ToString());
		}

		[TestMethod]
		public void FromStringContainsAllOptions()
		{
			var shortcut = KeyboardShortcut.FromString("A4+09:2");
			Assert.AreEqual(0x09, shortcut.VirtualKeyCode);
			Assert.AreEqual(0xA4, shortcut.Modifier);
			Assert.AreEqual(2, shortcut.ShortcutPressesBeforeOpen);
		}

		[TestMethod]
		public void FromEmptyStringEnabledIsFale()
		{
			var shortcut = KeyboardShortcut.FromString("");
			Assert.AreEqual(false, shortcut.Enabled);
		}
	}
}
