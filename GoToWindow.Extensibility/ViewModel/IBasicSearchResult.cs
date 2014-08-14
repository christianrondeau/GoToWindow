using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility.ViewModel
{
	public interface IBasicSearchResult
	{
		BitmapFrame Icon { get; }
		string Title { get; }
		string Process { get; }
	}
}
