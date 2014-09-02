using System;

namespace GoToWindow.Extensibility.ViewModel
{
	public interface IWindowSearchResult : IBasicSearchResult
	{
		new string Error { get; set; }
		IntPtr HWnd { get; }
	}
}
