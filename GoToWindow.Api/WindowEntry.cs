using System;

namespace GoToWindow.Api
{
	public class WindowEntry : IWindowEntry
	{
		public IntPtr HWnd { get; set; }
		public uint ProcessId { get; set; }
	    public string Title { get; set; }
        public IntPtr IconHandle { get; set; }
        public bool IsVisible { get; set; }
        public string ProcessName { get; set; }

		public bool Focus()
		{
			return WindowToForeground.ForceWindowToForeground(HWnd);
		}

		public bool IsForeground()
		{
			return WindowToForeground.GetForegroundWindow() == HWnd;
		}

		public bool IsSameWindow(IWindowEntry other)
		{
			if (other == null)
				return false;

			return ProcessId == other.ProcessId && HWnd == other.HWnd;
		}

		public override string ToString()
		{
            return String.Format("{0} ({1}): \"{2}\"", ProcessName, ProcessId, Title);
		}
	}
}
