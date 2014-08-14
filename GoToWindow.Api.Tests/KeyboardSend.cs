using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class KeyboardSendTests
	{
		[TestMethod]
		public void CanConvertNumberKeysToKeyCode()
		{
			Assert.AreEqual(KeyboardSend.GetNumber(0), 0x30);
			Assert.AreEqual(KeyboardSend.GetNumber(9), 0x39);
		}
	}
}
