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
			Assert.AreEqual(0xA4, shortcut.ControlVirtualKeyCode);
			Assert.AreEqual(2, shortcut.ShortcutPressesBeforeOpen);
		}

		[TestMethod]
		public void FromEmptyStringEnabledIsFalse()
		{
			var shortcut = KeyboardShortcut.FromString("");
			Assert.AreEqual(false, shortcut.Enabled);
		}

		[TestMethod]
		public void DoesNotTriggerShortcutIfControlKeyIsUp()
		{
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = 101,
				VirtualKeyCode = 102,
				ShortcutPressesBeforeOpen = 1
			};

			Assert.IsFalse(shortcut.ShortcutKeyDown());
			Assert.IsFalse(shortcut.ShortcutKeyUp());
		}

		[TestMethod]
		public void DoesNotHandleControlKeyIfShortcutIsNotPressed()
		{
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = 101,
				VirtualKeyCode = 102,
				ShortcutPressesBeforeOpen = 1
			};

			shortcut.ControlKeyDown();
			Assert.IsFalse(shortcut.ControlKeyUp());
		}

		[TestMethod]
		public void WhenConfiguredForSinglePress_ReturnsTrueForDownAndUp()
		{
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = 101,
				VirtualKeyCode = 102,
				ShortcutPressesBeforeOpen = 1
			};

			shortcut.ControlKeyDown();
			Assert.IsTrue(shortcut.ShortcutKeyDown());
			Assert.IsTrue(shortcut.ShortcutKeyUp());
			Assert.IsTrue(shortcut.ControlKeyUp());
			Assert.IsFalse(shortcut.ShortcutKeyDown());
			Assert.IsFalse(shortcut.ShortcutKeyUp());
		}

		[TestMethod]
		public void WhenConfiguredForDoublePress_ReturnsTrueForSecondDownAndUp()
		{
			var shortcut = new KeyboardShortcut
			{
				ControlVirtualKeyCode = 101,
				VirtualKeyCode = 102,
				ShortcutPressesBeforeOpen = 2
			};

			shortcut.ControlKeyDown();
			Assert.IsFalse(shortcut.ShortcutKeyDown());
			Assert.IsFalse(shortcut.ShortcutKeyUp());
			Assert.IsTrue(shortcut.ShortcutKeyDown());
			Assert.IsTrue(shortcut.ShortcutKeyUp());
			shortcut.ControlKeyUp();
		}
	}
}
