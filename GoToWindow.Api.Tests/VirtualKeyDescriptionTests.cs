using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class VirtualKeyDescriptionTests
	{
		[TestMethod]
		public void CanReadEnumValuesWithoutDescriptions()
		{
			Assert.AreEqual("Unnamed Key", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Custom));
			Assert.AreEqual("Unnamed Key", VirtualKeyDescription.GetDescription(KeyboardControlKeys.Undefined));
		}

		[TestMethod]
		public void CanReadVirtualKeys()
		{
			Assert.AreEqual("F1", VirtualKeyDescription.GetDescription((KeyboardVirtualKeys)0x70));
			Assert.AreEqual("F12", VirtualKeyDescription.GetDescription((KeyboardVirtualKeys)0x7B));
			Assert.AreEqual("Tab", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Tab));
			Assert.AreEqual("Escape", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Escape));
		}

		[TestMethod]
		public void CanReadControlKeys()
		{
			Assert.AreEqual("Left Win", VirtualKeyDescription.GetDescription(KeyboardControlKeys.LWin));
			Assert.AreEqual("Left Alt", VirtualKeyDescription.GetDescription(KeyboardControlKeys.LAlt));
			Assert.AreEqual("Left Control", VirtualKeyDescription.GetDescription(KeyboardControlKeys.LCtrl));
		}
	}
}