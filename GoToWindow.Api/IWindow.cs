using System;

namespace GoToWindow.Api
{
    public interface IWindow
    {
        IntPtr HWnd { get; set; }
        string ProcessName { get; set; }
        string Title { get; set; }

        void Focus();
    }
}
