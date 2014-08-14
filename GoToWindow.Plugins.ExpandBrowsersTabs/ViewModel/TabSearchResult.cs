using System;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.ViewModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel
{
    public class TabSearchResult : SearchResultBase, IBasicSearchResult, ISearchResult
    {
        private readonly IWindowSearchResult _item;
        private readonly ITab _tab;

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

        public TabSearchResult(IWindowSearchResult item, ITab tab, Func<UserControl> viewCtor)
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
