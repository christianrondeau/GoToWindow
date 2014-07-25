using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            var openedWindows = windows.Select(window => window.Title).ToArray();
            var containsVisualStudio = openedWindows.Count(title => title.StartsWith(expectedWindow)) >= 1;
            Assert.IsTrue(containsVisualStudio, String.Format("Expected a window to start with {0}. List:\r\n{1}", expectedWindow, String.Join("\r\n", openedWindows)));
        }
    }
}
