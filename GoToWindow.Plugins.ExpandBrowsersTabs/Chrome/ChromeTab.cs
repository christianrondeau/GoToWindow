using GoToWindow.Plugins.ExpandBrowsersTabs.Common;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Chrome
{
	public class ChromeTab : ShortcutBasedTab, ITab
	{
		public ChromeTab(string title, int tabIndex)
			: base(title, tabIndex)
		{
		}
	}
}
