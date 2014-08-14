using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Chrome
{
    /// <remarks>
    /// Thanks to CoenraadS: https://github.com/CoenraadS/Chrome-Tab-Switcher
    /// </remarks>
	public class ChromeTabsFinder : ITabsFinder
    {
		public IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd)
        {
            var tabs = new List<ChromeTab>();

			AutomationElement parent = AutomationElement.FromHandle(hWnd);

            AutomationElement mainElement = parent.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));

            if (mainElement == null)
                yield break;
            
            AutomationElement tabBarElement = mainElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab"));

            AutomationElementCollection tabElements = tabBarElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab item"));

            for (int tabIndex = 0; tabIndex < tabElements.Count; tabIndex++)
            {
                yield return new ChromeTab(tabElements[tabIndex].Current.Name, tabIndex + 1);
            }
        }
    }
}