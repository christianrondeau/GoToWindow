using System;
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
			Object o;
			if(_tabElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out o))
			{
				var pattern = o as SelectionItemPattern;
				pattern.Select();
			}
			else
			{
				var pos = _tabElement.GetClickablePoint();
				MouseSend.Click((int)pos.X, (int)pos.Y);
			}
		}
	}
}
