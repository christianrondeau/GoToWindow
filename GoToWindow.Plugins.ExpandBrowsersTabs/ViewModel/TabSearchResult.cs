using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel
{
	public class TabSearchResult : SearchResultBase, IBasicSearchResult
	{
		private readonly IWindowSearchResult _item;
		private readonly ITab _tab;

		public BitmapFrame Icon => _item.Icon;

		public string Title => _tab.Title;

		public string ProcessName => _item.ProcessName;

		public string Error => _item.Error;

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
			return IsShown(searchQuery, ProcessName, Title);
		}
	}
}
