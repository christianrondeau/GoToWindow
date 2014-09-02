using System;
using System.Diagnostics;
using log4net;

namespace GoToWindow.ViewModels
{
	public class PerformanceLogger : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PerformanceLogger).Assembly, "GoToWindow");

		private readonly Stopwatch _stopwatch;
		private readonly string _step;

		public PerformanceLogger(string step)
		{
			_step = step;
			
			if (!Log.IsDebugEnabled) return;
			
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
		}


		public void Dispose()
		{
			if (_stopwatch == null) return;

			_stopwatch.Stop();
			Log.InfoFormat("{0} took {1} to execute.", _step, _stopwatch.Elapsed);
		}
	}
}