using System;

namespace GoToWindow.Api
{
    public class Window : IWindow
    {
        public IntPtr IntPtr { get; set; }
        public string Title { get; set; }

        public void Focus()
        {
            WindowToForeground.ForceWindowToForeground(IntPtr);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
