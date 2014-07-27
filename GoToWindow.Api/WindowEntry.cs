using System;

namespace GoToWindow.Api
{
    public class WindowEntry : IWindowEntry
    {
        public IntPtr HWnd { get; set; }
        public string ProcessName { get; set; }
        public string Executable { get; set; }
        public string Title { get; set; }

        public void Focus()
        {
            WindowToForeground.ForceWindowToForeground(HWnd);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
