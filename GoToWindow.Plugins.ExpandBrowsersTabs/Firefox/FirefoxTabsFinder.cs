using System;
using System.Collections.Generic;
using System.Windows.Automation;
using GoToWindow.Plugins.ExpandBrowsersTabs.Common;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class FirefoxTabsFinder : UIAutomationTabsFinderBase, ITabsFinder
	{
		public IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd)
		{
			var firefoxWindow = AutomationElement.FromHandle(hWnd);

			var cacheRequest = new CacheRequest();
			cacheRequest.Add(AutomationElement.NameProperty);
			cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);
			cacheRequest.Add(SelectionItemPattern.Pattern);
			cacheRequest.Add(SelectionItemPattern.SelectionContainerProperty);
			cacheRequest.TreeScope = TreeScope.Element;

			AutomationElement tabBarElement;

			using (cacheRequest.Activate())
			{
				var mainElement = firefoxWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Browser tabs"));

				if (mainElement == null)
					yield break;

				tabBarElement = mainElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab"));
			}

			if(tabBarElement == null)
				yield break;

			var tabElements = tabBarElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab item"));

			for (var tabIndex = 0; tabIndex < tabElements.Count; tabIndex++)
			{
				yield return new FirefoxTab(tabElements[tabIndex].Current.Name, tabIndex + 1);
			}
		}
	}
}