using System;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using GoToWindow.Plugins.ExpandBrowsersTabs.Common;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class FirefoxTab : ShortcutBasedTab, ITab
	{
		public FirefoxTab(string title, int tabIndex)
			: base(title, tabIndex)
		{
		}
	}
}
