using System;
using System.Collections.Generic;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.InternetExplorer
{
	public class InternetExplorerTabsFinder : ITabsFinder
	{
		private bool _searchedOnce;

		public bool CanGetTabsOfWindow(IWindowSearchResult item, out string errorMessage)
		{
			errorMessage = null;
			return true;
		}

		public IEnumerable<ITab> GetTabsOfWindow(IntPtr chromeWindowHandle)
		{
			if (_searchedOnce)
				yield break;

			_searchedOnce = true;

			var shellWindows = new SHDocVw.ShellWindows();

			foreach (SHDocVw.InternetExplorer ie in shellWindows)
			{
				var document = ie.Document;

				if (document == null) continue;
				if (document.GetType().Name != "HTMLDocumentClass") continue;

				yield return new InternetExplorerTab(document.title ?? ie.LocationName, ie);
			}
		}
	}
}
