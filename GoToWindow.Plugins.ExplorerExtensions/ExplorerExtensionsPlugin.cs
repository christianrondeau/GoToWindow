using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Plugins.ExplorerExtensions.ViewModel;
using log4net;

namespace GoToWindow.Plugins.ExplorerExtensions
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class ExplorerExtensionsPlugin : IGoToWindowPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ExplorerExtensionsPlugin).Assembly, "GoToWindow");

		public string Id { get { return "GoToWindow.ExplorerExtensions"; } }

		public string Title { get { return "GoToWindow Explorer Extensions"; } }

		public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

		public void BuildList(List<ISearchResult> list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var result = list[i];
				var window = result as IWindowSearchResult;

				if (window != null && window.ProcessName == "explorer")
				{
					list[i] = ProcessExplorerWindow(window);
				}
			}

			list.Add(new OpenExplorerCommandResult());
		}

		private static IWindowSearchResult ProcessExplorerWindow(IWindowSearchResult window)
		{
			var t = Type.GetTypeFromProgID("Shell.Application");
			dynamic o = null;
			try
			{
				o = Activator.CreateInstance(t);
				var ws = o.Windows();
				for (var i = 0; i < ws.Count; i++)
				{
					var ie = ws.Item(i);
					if (ie == null || ie.hwnd != (long) window.HWnd) continue;
					var path = System.IO.Path.GetFileName((string) ie.FullName);
					if (path != null && path.ToLower() == "explorer.exe")
					{
						return new ExplorerWindowSearchResult(window, ie.document.focuseditem.path);
					}
				}
			}
			catch (Exception exc)
			{
				Log.Error("Could not extract path from explorer window", exc);
			}
			finally
			{
				if (o != null)
					Marshal.FinalReleaseComObject(o);
			}

			return window;
		}
	}
}
