using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace GoToWindow.Api
{
    /// <remarks>
    /// Thanks to http://stackoverflow.com/users/270315/jaroslaw-waliszko for the idea
    /// </remarks>
    public static class WmiProcessWatcher
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(WmiProcessWatcher).Assembly, "GoToWindow");

        private static IDictionary<uint, string> _map;
        private static ManagementEventWatcher _processStartedWatcher;
        private static ManagementEventWatcher _processStoppedwatcher;

        public static void Start()
        {
            if (!WindowsRuntimeHelper.GetHasElevatedPrivileges())
                return;

			try
			{
				_map = new ConcurrentDictionary<uint, string>();

				_processStartedWatcher = new ManagementEventWatcher("SELECT ProcessID, ProcessName FROM Win32_ProcessStartTrace");
				_processStartedWatcher.EventArrived += ProcessStarted;
				_processStartedWatcher.Start();

				_processStoppedwatcher = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStopTrace");
				_processStoppedwatcher.EventArrived += ProcessStopped;
				_processStoppedwatcher.Start();
			}
			catch (ManagementException exc)
			{
				Log.Warn("Could not start listening to WMI events", exc);
				Stop();
			}
        }

        private static void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            var processName = (string)e.NewEvent["ProcessName"];
            var processId = (uint)e.NewEvent["ProcessId"];

            if (string.IsNullOrEmpty(processName))
                return;

            _map[processId] = Path.GetFileNameWithoutExtension(processName);
        }

        private static void ProcessStopped(object sender, EventArrivedEventArgs e)
        {
            var processId = (uint)e.NewEvent["ProcessId"];

            _map.Remove(processId);
        }

        public static void Stop()
        {
            if (_processStartedWatcher != null)
            {
                _processStartedWatcher.Stop();
                _processStartedWatcher.Dispose();
                _processStartedWatcher = null;
            }

            if (_processStoppedwatcher != null)
            {
                _processStoppedwatcher.Stop();
                _processStoppedwatcher.Dispose();
                _processStoppedwatcher = null;
            }
        }

        public static string GetProcessName(uint processId, Func<string> getProcessName)
        {
            if (_map == null)
                return getProcessName();

			if (_map.TryGetValue(processId, out string processName))
			{
				return processName;
			}

			processName = getProcessName();
            _map[processId] = processName;
            return processName;
        }
    }
}