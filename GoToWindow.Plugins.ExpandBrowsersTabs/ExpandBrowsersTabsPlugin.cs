using System.Collections.Generic;
using System.Linq;
using GoToWindow.Extensibility;
using GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Plugins.ExpandBrowsersTabs.Chrome;
using System;

namespace GoToWindow.Plugins.ExpandBrowsersTabs
{
    [Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
    public class ExpandBrowsersTabsPlugin : IGoToWindowPlugin
    {
        public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

        public void BuildList(List<ISearchResult> list)
        {
            for(int index = list.Count - 1; index >= 0; index--)
            {
                var item = list[index] as IWindowSearchResult;

				if (item == null)
					continue;

				switch(item.Process)
				{
					case "chrome":
						ReplaceEntries(item, list, index, ChromeProcessToTabs);
						break;
				}
            }
        }

        private static void ReplaceEntries(IWindowSearchResult item, List<ISearchResult> list, int index, Func<IWindowSearchResult, IEnumerable<ISearchResult>> processConvertFunc)
        {
			list.RemoveAt(index);
			list.InsertRange(index, processConvertFunc(item));
        }

        private static IEnumerable<ISearchResult> ChromeProcessToTabs(IWindowSearchResult item)
        {
            var tabs = ChromeTabsFinder.GetTabsOfChromeWindow(item.HWnd);

            foreach (var tab in tabs)
            {
                yield return new ChromeTabSearchResult(item, tab, () => new BasicListEntry());
            }
        }
    }
}
