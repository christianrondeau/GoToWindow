using System;

namespace GoToWindow.Extensibility.ViewModel
{
	public interface IWindowSearchResult : ISearchResult, IBasicSearchResult
	{
		IntPtr HWnd { get; }
	}
}
