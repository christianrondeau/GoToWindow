using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GoToWindow.Api.Tests
{

    public class GivenAWindow : IDisposable
    {
        public static string GetResourcePath(string filename)
        {
            var executablePath = Path.GetDirectoryName(typeof(WindowListFactoryTests).Assembly.Location);
            return Path.Combine(executablePath, filename);
        }

        private Process _process;
        public IWindowEntry Expected { get; private set; }

        public GivenAWindow(string title)
        {
            var filename = GetResourcePath("GoToWindow.FakeApp.exe");
            var processName = "GoToWindow.FakeApp";

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = "\"" + title + "\""
                }
            };

            if (!_process.Start())
                throw new Exception("Could not start fake app");

            Thread.Sleep(1000); //TODO: Instead wait for some console output

            Expected = new WindowEntry
            {
                Title = title,
                ProcessName = processName,
                Executable = filename
            };
        }

        public void Dispose()
        {
            _process.Kill();
            _process.Dispose();
        }
    }
}
