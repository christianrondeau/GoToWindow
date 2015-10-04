using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoToWindow.Api.Tests
{
	[TestClass]
	public class WindowEntryFactoryTests
	{
		[TestMethod]
		public void GetGetWindowEntry_FromTestWindow()
		{
			using (var app = new GivenAnApp("GoToWindow.GetGetWindowEntry_FromTestWindow"))
			{
				var expectedWindowHandle = app.Process.MainWindowHandle;
				var window = WindowEntryFactory.Create(expectedWindowHandle);

				Assert.AreEqual(expectedWindowHandle, window.HWnd);
				Assert.AreEqual((uint)app.Process.Id, window.ProcessId);
				Assert.AreEqual(app.ExpectedWindow.Title, window.Title);
				Assert.AreNotEqual(IntPtr.Zero, window.IconHandle);
				Assert.IsNull(window.ProcessName);
			}
		}
	}
}
