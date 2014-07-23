using System;

namespace GoToWindow.Api
{
    public interface IWindow
    {
        IntPtr IntPtr { get; set; }
        string Title { get; set; }
    }
}
