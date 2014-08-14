using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility
{
    public interface ISearchResult
    {
        UserControl View { get; }
        void Select();
        bool IsShown(string searchQuery);
    }
}