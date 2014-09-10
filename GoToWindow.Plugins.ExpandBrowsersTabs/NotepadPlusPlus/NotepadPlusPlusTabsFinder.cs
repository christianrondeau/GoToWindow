using System;
using System.Collections.Generic;
using System.Windows.Automation;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class NotepadPlusPlusTabsFinder : ITabsFinder
	{
		public IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd)
		{
			var notepadPlusPlusWindow = AutomationElement.FromHandle(hWnd);

			var cacheRequest = new CacheRequest();
			cacheRequest.Add(AutomationElement.NameProperty);
			cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);
			cacheRequest.Add(SelectionItemPattern.Pattern);
			cacheRequest.Add(SelectionItemPattern.SelectionContainerProperty);
			cacheRequest.TreeScope = TreeScope.Element;

			AutomationElement tabBarElement;

			using (cacheRequest.Activate())
			{
				tabBarElement = notepadPlusPlusWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab"));
			}

			if(tabBarElement == null)
				yield break;

			var tabElements = tabBarElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab item"));

			for (var tabIndex = 0; tabIndex < tabElements.Count; tabIndex++)
			{
				yield return new NotepadPlusPlusTab(tabElements[tabIndex]);
			}
		}
	}
}