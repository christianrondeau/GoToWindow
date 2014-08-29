using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Automation;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Firefox
{
	public class FirefoxTabsFinder : ITabsFinder
	{
		public IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd)
		{
			var chromeWindow = AutomationElement.FromHandle(hWnd);

			var mainElement = chromeWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Browser tabs"));

			if (mainElement == null)
				yield break;

			var tabBarElement = mainElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab"));

			var tabElements = tabBarElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tab item"));

			for (var tabIndex = 0; tabIndex < tabElements.Count; tabIndex++)
			{
				yield return new FirefoxTab(tabElements[tabIndex].Current.Name, tabIndex + 1);
			}

			/*
			AutomationElement rootElement = AutomationElement.FromHandle(hWnd);

			Condition condCustomControl = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom);
			AutomationElement firstCustomControl = GetNextCustomControl(rootElement, condCustomControl);
			AutomationElement secondCustomControl = GetNextCustomControl(firstCustomControl, condCustomControl);
			foreach (AutomationElement thirdElement in secondCustomControl.FindAll(TreeScope.Children, condCustomControl))
			{
				foreach (AutomationElement fourthElement in thirdElement.FindAll(TreeScope.Children, condCustomControl))
				{
					Condition condDocument = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document);
					AutomationElement docElement = fourthElement.FindFirst(TreeScope.Children, condDocument);
					if (docElement != null)
					{
						foreach (AutomationPattern pattern in docElement.GetSupportedPatterns())
						{
							var valuePattern = docElement.GetCurrentPattern(pattern) as ValuePattern;
							if (valuePattern != null)
							{
								yield return new FirefoxTab(valuePattern.Current.Value, 0);
							}
						}
					}
				}
			}*/
		}
		/*
		private static AutomationElement GetNextCustomControl(AutomationElement rootElement, Condition condCustomControl)
		{
			return rootElement.FindAll(TreeScope.Children, condCustomControl).Cast<AutomationElement>().ToList().Where(x => x.Current.BoundingRectangle != System.Windows.Rect.Empty).FirstOrDefault();
		}*/
	}
}