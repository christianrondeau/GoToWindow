using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowTests
    {
        [TestMethod]
        public void ToString_GetsTheWindowTitle()
        {
            Assert.AreEqual("Title [process:path]", new WindowEntry { Title = "Title", ProcessName = "process", Executable = "path" }.ToString());
        }
    }
}
