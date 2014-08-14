using System;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.ViewModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using GoToWindow.Plugins.ExpandBrowsersTabs.Chrome;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel
{
    public class ChromeTabSearchResult : SearchResultBase, IBasicSearchResult, ISearchResult
    {
        private readonly IWindowSearchResult _item;
        private readonly ChromeTab _tab;

        public BitmapFrame Icon
        {
            get { return _item.Icon; }
        }

        public string Title
        {
            get { return _tab.Title; }
        }

        public string Process
        {
            get { return _item.Process; }
        }

        public ChromeTabSearchResult(IWindowSearchResult item, ChromeTab tab, Func<UserControl> viewCtor)
            : base(viewCtor)
        {
            _item = item;
            _tab = tab;
        }

        public void Select()
        {
            _item.Select();
            _tab.Select();
        }

        public bool IsShown(string searchQuery)
        {
            return IsShown(searchQuery, Process, Title);
        }
    }
}
