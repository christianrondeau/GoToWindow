using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class VirtualKeyDescriptionTests
	{
		[TestMethod]
		public void CanReadEnumValuesWithoutDescriptions()
		{
			Assert.AreEqual("?", VirtualKeyDescription.GetVirtualKeyDescription((int)KeyboardVirtualKeys.Custom));
			Assert.AreEqual("?", VirtualKeyDescription.GetVirtualKeyDescription((int)KeyboardControlKeys.Undefined));
		}

		[TestMethod]
		public void CanReadVirtualKeys()
		{
			Assert.AreEqual("F1", VirtualKeyDescription.GetVirtualKeyDescription(0x70));
			Assert.AreEqual("F12", VirtualKeyDescription.GetVirtualKeyDescription(0x7B));
			Assert.AreEqual("Tab", VirtualKeyDescription.GetVirtualKeyDescription((int)KeyboardVirtualKeys.Tab));
			Assert.AreEqual("Esc", VirtualKeyDescription.GetVirtualKeyDescription((int)KeyboardVirtualKeys.Escape));
			Assert.AreEqual("6", VirtualKeyDescription.GetVirtualKeyDescription(0x36));
		}

		[TestMethod]
		public void CanReadControlKeys()
		{
			Assert.AreEqual("Left Win", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)KeyboardControlKeys.LWin));
			Assert.AreEqual("Left Alt", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)KeyboardControlKeys.LAlt));
			Assert.AreEqual("Left Control", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)KeyboardControlKeys.LCtrl));
			Assert.AreEqual("Left Shift", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)KeyboardControlKeys.LShift));
		}
	}
}