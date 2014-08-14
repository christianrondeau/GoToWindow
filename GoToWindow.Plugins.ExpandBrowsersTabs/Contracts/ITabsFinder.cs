using System;
using System.Collections.Generic;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public interface ITabsFinder
	{
		IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd);
	}
}
