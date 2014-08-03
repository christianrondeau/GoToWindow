using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility
{
    public interface IGoToWindowSearchResult
    {
        BitmapFrame Icon { get; }
        string Title { get; }
        string Process { get; }
        void Select();
        bool IsShown(string searchQuery);
    }
}