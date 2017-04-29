using System.Windows.Automation;
using GoToWindow.Api;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.NotepadPlusPlus
{
	public class NotepadPlusPlusTab : TabBase, ITab
	{
		private readonly AutomationElement _tabElement;

		public NotepadPlusPlusTab(AutomationElement tabElement)
			: base(tabElement.Current.Name)
		{
			_tabElement = tabElement;
		}

		public void Select()
		{
			if (_tabElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object o))
			{
				((SelectionItemPattern)o).Select();
			}
			else
			{
				var pos = _tabElement.GetClickablePoint();
				MouseSend.Click((int)pos.X, (int)pos.Y);
			}
		}
	}
}
