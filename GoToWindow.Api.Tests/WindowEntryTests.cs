using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowEntryTests
    {
        [TestMethod]
        public void CanGiveFocusToAWindow()
        {
            using (var app1 = new GivenAnApp("GoToWindow.GetGetWindowEntry_FromTestWindow1"))
            {
                var window1 = WindowEntryFactory.Create(app1.Process.MainWindowHandle);

                using (var app2 = new GivenAnApp("GoToWindow.GetGetWindowEntry_FromTestWindow2"))
                {
                    var window2 = WindowEntryFactory.Create(app2.Process.MainWindowHandle);

                    Assert.IsFalse(window1.HasFocus());
                    Assert.IsTrue(window2.HasFocus());

                    window1.Focus();

                    Assert.IsTrue(window1.HasFocus());
                }
                
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
