using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility.ViewModel
{
	public interface IBasicSearchResult : ISearchResult
	{
		BitmapFrame Icon { get; }
		string Title { get; }
		string ProcessName { get; }
		string Error { get; }
	}
}
