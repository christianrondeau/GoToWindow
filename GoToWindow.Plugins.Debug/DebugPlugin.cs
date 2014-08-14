using System.Collections.Generic;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility;
using GoToWindow.Plugins.Debug.Controls;
using GoToWindow.Plugins.Debug.ViewModel;

namespace GoToWindow.Plugins.Debug
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class DebugPlugin : IGoToWindowPlugin
	{
		public string Id { get { return "GoToWindow.Debug"; } }

		public string Title { get { return "GoToWindow Debugger"; } }

		public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

		public void BuildList(List<ISearchResult> list)
		{
			list.Insert(0, new DebugSearchResult(
				string.Format("Debug: Found {0} entries", list.Count),
				new DebugListEntry())
				);
		}
	}
}
