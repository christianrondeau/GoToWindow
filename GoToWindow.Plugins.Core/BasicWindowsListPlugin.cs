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
    public class BasicWindowsListPlugin : IGoToWindowPlugin
    {
        public IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList()
        {
            return WindowsListFactory.Load().Windows.Select(ConvertWindowEntryToSearchResult);
        }

        private static IGoToWindowSearchResult ConvertWindowEntryToSearchResult(IWindowEntry entry)
        {
            return new BasicWindowSearchResult(entry, new WindowListEntry());
        }
    }
}
