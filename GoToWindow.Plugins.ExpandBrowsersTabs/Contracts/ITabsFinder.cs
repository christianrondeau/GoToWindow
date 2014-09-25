using System;
using System.Collections.Generic;
using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public interface ITabsFinder
	{
	    bool CanGetTabsOfWindow(IWindowSearchResult item, out string errorMessage);
	    IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd);
	}
}
