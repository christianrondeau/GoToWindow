using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class VirtualKeyDescriptionTests
	{
		[TestMethod]
		public void CanReadEnumValuesWithoutDescriptions()
		{
			Assert.AreEqual("?", VirtualKeyDescription.GetVirtualKeyDescription(0));
			Assert.AreEqual("?", VirtualKeyDescription.GetVirtualKeyDescription(-1));
		}

		[TestMethod]
		public void CanReadVirtualKeys()
		{
			Assert.AreEqual("F1", VirtualKeyDescription.GetVirtualKeyDescription(0x70));
			Assert.AreEqual("F12", VirtualKeyDescription.GetVirtualKeyDescription(0x7B));
			Assert.AreEqual("Tab", VirtualKeyDescription.GetVirtualKeyDescription(0x09));
			Assert.AreEqual("Esc", VirtualKeyDescription.GetVirtualKeyDescription(0x1B));
			Assert.AreEqual("6", VirtualKeyDescription.GetVirtualKeyDescription(0x36));
		}

		[TestMethod]
		public void CanReadControlKeys()
		{
			Assert.AreEqual("Left Win", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)ModifierVirtualKeys.LWin));
			Assert.AreEqual("Left Alt", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)ModifierVirtualKeys.LAlt));
			Assert.AreEqual("Left Control", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)ModifierVirtualKeys.LCtrl));
			Assert.AreEqual("Left Shift", VirtualKeyDescription.GetModifierVirtualKeyDescription((int)ModifierVirtualKeys.LShift));
		}
	}
}