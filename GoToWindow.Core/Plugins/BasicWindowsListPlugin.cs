using System.Collections.Generic;
using System.Linq;
using GoToWindow.Api;
using GoToWindow.Extensibility;

namespace GoToWindow.Core.Plugins
{
    public class BasicWindowsListPlugin : IGoToWindowPlugin
    {
        public IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList()
        {
            return WindowsListFactory.Load().Windows.Select(ConvertWindowEntryToSearchResult);
        }

        private static IGoToWindowSearchResult ConvertWindowEntryToSearchResult(IWindowEntry entry)
        {
            return new BasicWindowSearchResult(entry);
        }
    }
}
