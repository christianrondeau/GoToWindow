using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GoToWindow.Api;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Plugins.Core.ViewModel;

namespace GoToWindow.Plugins.Core
{
	[Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
	public class CorePlugin : IGoToWindowPlugin
	{
		public string Id => "GoToWindow.Core";

		public string Title => "GoToWindow Core";

		public GoToWindowPluginSequence Sequence => GoToWindowPluginSequence.Core;

		public void BuildList(List<ISearchResult> list)
		{
			list.AddRange(WindowsListFactory.Load().Windows.Select(ConvertWindowEntryToSearchResult));
			list.Add(new WindowSearchCommandResult());
		}

		private static ISearchResult ConvertWindowEntryToSearchResult(IWindowEntry entry)
		{
			return new CoreSearchResult(entry, () => new BasicListEntry());
		}
	}
}
