using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoToWindow.Api;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowTests
    {
        [TestMethod]
        public void ToString_GetsTheWindowTitle()
        {
            const string expectedTitle = "Some App: Some Window Title";

            Assert.AreEqual(expectedTitle, new Window { Title = expectedTitle }.ToString());
        }
    }
}
