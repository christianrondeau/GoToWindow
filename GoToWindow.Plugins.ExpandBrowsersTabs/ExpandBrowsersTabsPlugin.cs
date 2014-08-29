using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel;
using GoToWindow.Plugins.ExpandBrowsersTabs.Chrome;
using GoToWindow.Plugins.ExpandBrowsersTabs.Firefox;
using GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer;

namespace GoToWindow.Plugins.ExpandBrowsersTabs
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class ExpandBrowsersTabsPlugin : IGoToWindowPlugin
	{
		public string Id { get { return "GoToWindow.ExpandBrowsersTabs"; } }

		public string Title { get { return "GoToWindow Expand Browser Tabs"; } }

		public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

		private static readonly IDictionary<string, Func<ITabsFinder>> TabsFinders = new Dictionary<string, Func<ITabsFinder>>
		{
			{ "chrome", () => new ChromeTabsFinder() },
			{ "iexplore", () => new InternetExplorerTabsFinder() },
			{ "firefox", () => new FirefoxTabsFinder() }
		};

		public void BuildList(List<ISearchResult> list)
		{
			var finders = new Dictionary<string, ITabsFinder>();

			for(int index = list.Count - 1; index >= 0; index--)
			{
				var item = list[index] as IWindowSearchResult;

				if (item == null)
					continue;

				ITabsFinder finder;
				if(!finders.TryGetValue(item.Process, out finder))
				{
					Func<ITabsFinder> finderCtor;
					if (TabsFinders.TryGetValue(item.Process, out finderCtor))
						finder = finderCtor();
					else
						continue;
				}

				var tabs = finder.GetTabsOfWindow(item.HWnd);

				list.RemoveAt(index);
				list.InsertRange(index, tabs.Select(tab => ConvertTabToResult(item, tab)));
			}
		}

		private static ISearchResult ConvertTabToResult(IWindowSearchResult item, ITab tab)
		{
			return new TabSearchResult(item, tab, () => new BasicListEntry());
		}
	}
}
