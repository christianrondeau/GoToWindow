using System;

namespace GoToWindow.Api
{
    public interface IWindowEntry
    {
        IntPtr HWnd { get; set; }
        string ProcessName { get; set; }
        string Executable { get; set; }
        string Title { get; set; }

        bool Focus();
    }
}
