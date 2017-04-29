using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GoToWindow.Api.Tests
{
	public class GivenAnApp : IDisposable
	{
		public static string GetResourcePath(string filename)
		{
			var executablePath = Path.GetDirectoryName(typeof(WindowListFactoryTests).Assembly.Location);
            return executablePath != null ? Path.Combine(executablePath, filename) : null;
		}

		public IWindowEntry ExpectedWindow { get; }
		public Process Process { get; }
		public string Executable { get; }

		public GivenAnApp(string title)
		{
			Executable = GetResourcePath("GoToWindow.FakeApp.exe");
			const string processName = "GoToWindow.FakeApp";

			Process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Executable,
					Arguments = "\"" + title + "\""
				}
			};

			if (!Process.Start())
				throw new Exception("Could not start fake app");

			Thread.Sleep(1000); //TODO: Instead wait for some console output

			ExpectedWindow = new WindowEntry
			{
				Title = title,
				ProcessId = (uint)Process.Id,
				ProcessName = processName,
				HWnd = Process.MainWindowHandle
			};
		}

		public void Dispose()
		{
			Process.Kill();
			Process.Dispose();
		}
	}
}
