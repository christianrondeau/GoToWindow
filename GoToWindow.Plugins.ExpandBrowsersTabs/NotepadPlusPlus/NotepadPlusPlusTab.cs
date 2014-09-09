using GoToWindow.Plugins.ExpandBrowsersTabs.Common;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class NotepadPlusPlusTab : ShortcutBasedTab, ITab
	{
		public NotepadPlusPlusTab(string title, int tabIndex)
			: base(title, tabIndex)
		{
		}
	}
}
