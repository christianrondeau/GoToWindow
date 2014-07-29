using System;

namespace GoToWindow.Api
{
    public class WindowEntry : IWindowEntry
    {
        public IntPtr HWnd { get; set; }
        public string ProcessName { get; set; }
        public string Executable { get; set; }
        public string Title { get; set; }
        public IntPtr IconHandle { get; set; }

        public bool Focus()
        {
            return WindowToForeground.ForceWindowToForeground(HWnd);
        }

        public bool IsSameButHWnd(IWindowEntry other)
        {
            if (other == null)
                return false;

            return ProcessName == other.ProcessName && Executable == other.Executable && Title == other.Title;
        }

        public override string ToString()
        {
            return String.Format("{0} [{1}:{2}]", Title, ProcessName, Executable);
        }
    }
}
