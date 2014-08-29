using GoToWindow.Plugins.ExpandBrowsersTabs.Common;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

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
