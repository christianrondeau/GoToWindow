using System.Collections.Generic;
using System.Linq;
using GoToWindow.Api;
using GoToWindow.Extensibility;
using GoToWindow.Plugins.Core.ViewModel;
using GoToWindow.Plugins.Core.Controls;
using System.ComponentModel.Composition;

namespace GoToWindow.Plugins.Core
{
    [Export(GoToWindowPluginConstants.GoToWindowPluginContractName, typeof(IGoToWindowPlugin))]
    public class CorePlugin : IGoToWindowPlugin
    {
        public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.Core; } }

        public void BuildList(List<IGoToWindowSearchResult> list)
        {
            list.AddRange(WindowsListFactory.Load().Windows.Select(ConvertWindowEntryToSearchResult));
        }

        private static IGoToWindowSearchResult ConvertWindowEntryToSearchResult(IWindowEntry entry)
        {
            return new CoreSearchResult(entry, new CoreListEntry());
        }
    }
}
