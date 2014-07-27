using System;

namespace GoToWindow.Api
{
    public class WindowEntry : IWindowEntry
    {
        public IntPtr HWnd { get; set; }
        public string ProcessName { get; set; }
        public string Executable { get; set; }
        public string Title { get; set; }

        public bool Focus()
        {
            return WindowToForeground.ForceWindowToForeground(HWnd);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
