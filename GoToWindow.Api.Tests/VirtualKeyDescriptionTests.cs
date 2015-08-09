using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class VirtualKeyDescriptionTests
	{
		[TestMethod]
		public void CanReadEnumValuesWithoutDescriptions()
		{
			Assert.AreEqual("Unnamed Key", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Undefined));
			Assert.AreEqual("Unnamed Key", VirtualKeyDescription.GetDescription(KeyboardControlKeys.Undefined));
		}

		[TestMethod]
		public void CanReadVirtualKeys()
		{
			Assert.AreEqual("Tab", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Tab));
			Assert.AreEqual("~", VirtualKeyDescription.GetDescription(KeyboardVirtualKeys.Console));
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