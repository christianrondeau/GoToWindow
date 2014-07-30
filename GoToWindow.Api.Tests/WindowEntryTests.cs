using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowEntryTests
    {
        [TestMethod]
        public void CanGiveFocusToAWindow()
        {
            using (var app = new GivenAnApp("GoToWindow.GetGetWindowEntry_FromTestWindow"))
            {
                var window = WindowEntryFactory.Create(app.Process.MainWindowHandle);
                window.Focus();
            }

            Assert.IsTrue(true, "This test just ensures the code doesn't crash");
        }

        [TestMethod]
        public void ToString_GetsTheWindowTitle()
        {
            Assert.AreEqual("Title [process:path]", new WindowEntry { Title = "Title", ProcessName = "process", Executable = "path" }.ToString());
        }
    }
}
