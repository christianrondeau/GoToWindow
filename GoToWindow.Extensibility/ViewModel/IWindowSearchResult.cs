using System;

namespace GoToWindow.Extensibility.ViewModel
{
	public interface IWindowSearchResult : IBasicSearchResult
	{
        bool IsVisible { get; }
		IntPtr HWnd { get; }
	    void SetError(string message);
	}
}
