using System.Windows.Automation;
using GoToWindow.Plugins.ExpandBrowsersTabs.Common;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using System.Runtime.InteropServices;
using System;
using GoToWindow.Api;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class NotepadPlusPlusTab : TabBase, ITab
	{
		private AutomationElement _tabElement;

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
