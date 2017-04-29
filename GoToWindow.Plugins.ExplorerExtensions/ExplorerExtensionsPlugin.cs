using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExplorerExtensions.ViewModel;
using log4net;
using SHDocVw;

namespace GoToWindow.Plugins.ExplorerExtensions
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class ExplorerExtensionsPlugin : IGoToWindowPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ExplorerExtensionsPlugin).Assembly, "GoToWindow");
		private InternetExplorer[] _explorerWindows;

		public string Id => "GoToWindow.ExplorerExtensions";

		public string Title => "GoToWindow Explorer Extensions";

		public GoToWindowPluginSequence Sequence => GoToWindowPluginSequence.AfterCore;

		public void BuildList(List<ISearchResult> list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var result = list[i];
				var window = result as IWindowSearchResult;

				if (window == null || window.ProcessName != "explorer") continue;

				UpdateExplorerWindows();
				list[i] = ProcessExplorerWindow(window);
			}

			list.Add(new OpenExplorerCommandResult());

			_explorerWindows = null;
		}

		private void UpdateExplorerWindows()
		{
			if (_explorerWindows != null)
				return;

			var shellWindows = new ShellWindows();

			_explorerWindows = shellWindows.Cast<InternetExplorer>().ToArray();
		}

		private IWindowSearchResult ProcessExplorerWindow(IWindowSearchResult window)
		{
			try
			{
				var explorer = _explorerWindows.FirstOrDefault(hwnd => hwnd.HWND == (int)window.HWnd);
				if (explorer == null)
					return window;

				var locationUrl = explorer.LocationURL;

				if (string.IsNullOrEmpty(locationUrl) || !Uri.IsWellFormedUriString(locationUrl, UriKind.Absolute))
					return window;

				return new ExplorerWindowSearchResult(window, new Uri(locationUrl).LocalPath);
			}
			catch (Exception exc)
			{
				Log.Error("Could not extract path from explorer window", exc);
			}

			return window;
		}
	}
}
