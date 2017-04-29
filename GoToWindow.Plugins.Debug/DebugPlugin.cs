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
		public string Id => "GoToWindow.Debug";

		public string Title => "GoToWindow Debugger";

		public GoToWindowPluginSequence Sequence => GoToWindowPluginSequence.AfterCore;

		public void BuildList(List<ISearchResult> list)
		{
			list.Insert(0, new DebugSearchResult(
				new DebugListEntry())
				);
		}
	}
}
