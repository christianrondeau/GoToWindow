using System.Collections.Generic;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility;

namespace GoToWindow.Plugins.ExplorerExtensions
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class ExplorerExtensionsPlugin : IGoToWindowPlugin
	{
		public string Id { get { return "GoToWindow.ExplorerExtensions"; } }

		public string Title { get { return "GoToWindow Expand Browser Tabs"; } }

		public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

		public void BuildList(List<ISearchResult> list)
		{
			/*
			     IntPtr MyHwnd = FindWindow(null, "Directory");
    var t = Type.GetTypeFromProgID("Shell.Application");
    dynamic o = Activator.CreateInstance(t);
    try
    {
        var ws = o.Windows();
        for (int i = 0; i < ws.Count; i++)
        {
            var ie = ws.Item(i);
            if (ie == null || ie.hwnd != (long)MyHwnd) continue;
            var path = System.IO.Path.GetFileName((string)ie.FullName);
            if (path.ToLower() == "explorer.exe")
            {
                var explorepath = ie.document.focuseditem.path;
            }
        }
    }
    finally
    {
        Marshal.FinalReleaseComObject(o);
    } 
			 */
		}
	}
}
