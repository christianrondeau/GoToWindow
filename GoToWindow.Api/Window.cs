using System;

namespace GoToWindow.Api
{
    public class Window : IWindow
    {
        public IntPtr HWnd { get; set; }
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
