using System;

namespace GoToWindow.Api
{
    public interface IWindowEntry
    {
        IntPtr HWnd { get; set; }
        uint ProcessId { get; set; }
        string ProcessName { get; set; }
        string Executable { get; set; }
        string Title { get; set; }
        IntPtr IconHandle { get; set; }

        bool Focus();
        bool IsSameButHWnd(IWindowEntry other);
    }
}
