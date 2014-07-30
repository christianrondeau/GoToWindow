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
            return Path.Combine(executablePath, filename);
        }

        private readonly Process _process;
        private string _executable;
        public IWindowEntry ExpectedWindow { get; private set; }
        public Process Process { get { return _process; } }
        public string Executable { get { return _executable; } }

        public GivenAnApp(string title)
        {
            _executable = GetResourcePath("GoToWindow.FakeApp.exe");
            const string processName = "GoToWindow.FakeApp";

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _executable,
                    Arguments = "\"" + title + "\""
                }
            };

            if (!_process.Start())
                throw new Exception("Could not start fake app");

            Thread.Sleep(1000); //TODO: Instead wait for some console output

            ExpectedWindow = new WindowEntry
            {
                Title = title,
                ProcessName = processName,
                Executable = _executable
            };
        }

        public void Dispose()
        {
            _process.Kill();
            _process.Dispose();
        }
    }
}
