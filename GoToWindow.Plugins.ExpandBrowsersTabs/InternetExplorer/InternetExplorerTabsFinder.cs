using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using System;
using System.Collections.Generic;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer
{
	public class InternetExplorerTabsFinder : ITabsFinder
	{
		private bool _searchedOnce = false;

		public IEnumerable<ITab> GetTabsOfWindow(IntPtr chromeWindowHandle)
		{
			if (_searchedOnce)
				yield break;

			_searchedOnce = true;

			var shellWindows = new SHDocVw.ShellWindows();

			foreach (SHDocVw.InternetExplorer ie in shellWindows)
			{
				var document = ie.Document;

				if(document == null || document.GetType().Name != "HTMLDocumentClass")
					continue;

				yield return new InternetExplorerTab(document.title ?? ie.LocationName, ie);
			}
		}
	}
}
