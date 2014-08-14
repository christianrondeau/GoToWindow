using System.Windows.Controls;

namespace GoToWindow.Extensibility
{
	public interface ISearchResult
	{
		UserControl View { get; }
		void Select();
		bool IsShown(string searchQuery);
	}
}