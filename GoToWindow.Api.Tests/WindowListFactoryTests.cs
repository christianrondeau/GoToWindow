using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoToWindow.Api;

namespace GoToWindow.Api.Tests
{
    [TestClass]
    public class WindowListFactoryTests
    {
        [TestMethod]
        public void CanGetAListOfActiveWindows_ContainingVisualStudio()
        {
            const string expectedWindow = "GoToWindow - Microsoft Visual Studio";

            var windowsList = WindowsListFactory.Load();
            var windows = windowsList.Windows;

            var openedWindows = windows.Select(window => window.Title);
            var containsVisualStudio = openedWindows.Where(title => title.StartsWith(expectedWindow)).Count() >= 1;
            Assert.IsTrue(containsVisualStudio, String.Format("Expected a window to start with {0}. List:\r\n{1}", expectedWindow, String.Join("\r\n", openedWindows)));
        }
    }
}
