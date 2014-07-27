using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowTests
    {
        [TestMethod]
        public void ToString_GetsTheWindowTitle()
        {
            const string expectedTitle = "Some App: Some Window Title";

            Assert.AreEqual(expectedTitle, new WindowEntry { Title = expectedTitle }.ToString());
        }
    }
}
