using System;
using System.Collections.Generic;
using System.Windows.Automation;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using GoToWindow.Plugins.ExpandBrowsersTabs.Common;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Chrome
{
	/// <remarks>
	/// Thanks to CoenraadS: https://github.com/CoenraadS/Chrome-Tab-Switcher
	/// </remarks>
	public class ChromeTabsFinder : UIAutomationTabsFinderBase, ITabsFinder
	{
		public IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd)
		{
			var cacheRequest = new CacheRequest();
			cacheRequest.Add(AutomationElement.NameProperty);
			cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);
			cacheRequest.Add(SelectionItemPattern.Pattern);
			cacheRequest.Add(SelectionItemPattern.SelectionContainerProperty);
			cacheRequest.TreeScope = TreeScope.Element;

			AutomationElement tabBarElement;

			using (cacheRequest.Activate())
			{
				var chromeWindow = AutomationElement.FromHandle(hWnd);

				var mainElement = chromeWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));

				if (mainElement == null)
					yield break;

				tabBarElement = mainElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab"));
			}

			if(tabBarElement == null)
				yield break;

			var tabElements = tabBarElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab item"));

			for (var tabIndex = 0; tabIndex < tabElements.Count; tabIndex++)
			{
				yield return new ChromeTab(tabElements[tabIndex].Current.Name, tabIndex + 1);
			}
		}
	}
}