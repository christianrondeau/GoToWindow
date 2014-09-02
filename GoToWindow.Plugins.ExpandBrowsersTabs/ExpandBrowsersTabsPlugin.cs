using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExpandBrowsersTabs.Chrome;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using GoToWindow.Plugins.ExpandBrowsersTabs.Firefox;
using GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer;
using GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel;
using log4net;

namespace GoToWindow.Plugins.ExpandBrowsersTabs
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class ExpandBrowsersTabsPlugin : IGoToWindowPlugin
	{
		private const int TimeoutMilliseconds = 1500;

		private static readonly ILog Log = LogManager.GetLogger(typeof(ExpandBrowsersTabsPlugin).Assembly, "GoToWindow");

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

			for (var index = list.Count - 1; index >= 0; index--)
			{
				var item = list[index] as IWindowSearchResult;

				if (item == null)
					continue;

				ITabsFinder finder;
				var browserName = item.ProcessName;
				if (!finders.TryGetValue(browserName, out finder))
				{
					Func<ITabsFinder> finderCtor;
					if (TabsFinders.TryGetValue(browserName, out finderCtor))
						finder = finderCtor();
					else
						continue;
				}

				IEnumerable<ITab> tabs = null;

				var tokenSource = new CancellationTokenSource();
				var token = tokenSource.Token;

				var task = Task.Factory.StartNew(() => tabs = finder.GetTabsOfWindow(item.HWnd).ToArray(), token);

				if (!task.Wait(TimeoutMilliseconds, token))
				{
					Log.WarnFormat("Timeout trying to get tabs for '{0}'", browserName);
					continue;
				}

				var tabResults = tabs.Select(tab => ConvertTabToResult(item, tab)).ToArray();

				if (tabResults.Length <= 0) continue;

				list.RemoveAt(index);
				list.InsertRange(index, tabResults);
			}
		}

		private static ISearchResult ConvertTabToResult(IWindowSearchResult item, ITab tab)
		{
			return new TabSearchResult(item, tab, () => new BasicListEntry());
		}
	}
}
