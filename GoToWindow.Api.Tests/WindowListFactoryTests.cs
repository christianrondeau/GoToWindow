using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowListFactoryTests
    {
        [TestMethod]
        public void CanGetAListOfActiveWindows_VerifyTestProcess()
        {
            using (var app = new GivenAnApp("GoToWindow.CanGetAListOfActiveWindows_VerifyTestProcess"))
            {
                var windowsList = WindowsListFactory.Load();
                var windows = windowsList.Windows;

                AssertExists(windows, app.ExpectedWindow);
            }
        }

        private void AssertExists(IList<IWindowEntry> windows, IWindowEntry expected)
        {
            var containsExpected = windows.Any(expected.IsSameWindow);
            Assert.IsTrue(containsExpected, String.Format("Expected window {0}.\r\nWindows List:\r\n{1}", expected, String.Join("\r\n", windows)));
        }
    }
}
