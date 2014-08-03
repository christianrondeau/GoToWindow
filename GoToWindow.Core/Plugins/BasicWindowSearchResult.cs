using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;
using GoToWindow.Api;
using GoToWindow.Core.Utils;
using GoToWindow.Extensibility;

namespace GoToWindow.Core.Plugins
{
    public class BasicWindowSearchResult : IGoToWindowSearchResult
    {
        private readonly IWindowEntry _entry;
        private BitmapFrame _icon;

        public BitmapFrame Icon { get { return _icon ?? (_icon = LoadIcon()); } }
        public string Title { get { return _entry.Title; } }
        public string Process { get { return _entry.ProcessName; } }

        public BasicWindowSearchResult(IWindowEntry entry)
        {
            _entry = entry;
        }

        public void Select()
        {
            _entry.Focus();
        }

        public bool IsShown(string searchQuery)
        {
            return StringContains(Process + " " + Title, searchQuery);
        }

        private BitmapFrame LoadIcon()
        {
            return IconLoader.LoadIcon(_entry.IconHandle, _entry.Executable);
        }

        private static bool StringContains(string text, string searchQuery)
        {
            return searchQuery.Split(' ').All(word => CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, word, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) > -1);
        }
    }
}