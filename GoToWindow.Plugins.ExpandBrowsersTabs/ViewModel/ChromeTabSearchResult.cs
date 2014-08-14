using System;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.ViewModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel
{
    public class ChromeTabSearchResult : SearchResultBase, IBasicSearchResult, ISearchResult
    {
        private readonly IBasicSearchResult _item;

        public BitmapFrame Icon
        {
            get { return _item.Icon; }
        }

        public string Title
        {
            get { return _item.Title; }
        }

        public string Process
        {
            get { return _item.Process; }
        }

        public ChromeTabSearchResult(IBasicSearchResult item, Func<UserControl> viewCtor)
            : base(viewCtor)
        {
            _item = item;
        }

        public void Select()
        {
            
        }

        public bool IsShown(string searchQuery)
        {
            return IsShown(searchQuery, Process, Title);
        }
    }
}
