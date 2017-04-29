using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			Assert.IsTrue(containsExpected, $"Expected window {expected}.\r\nWindows List:\r\n{string.Join("\r\n", windows)}");
		}
	}
}
