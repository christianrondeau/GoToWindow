using System.Collections.Generic;
using System.Linq;
using GoToWindow.Extensibility;
using GoToWindow.Plugins.Debug.ViewModel;
using GoToWindow.Plugins.Debug.Controls;
using System.ComponentModel.Composition;

namespace GoToWindow.Plugins.Debug
{
    [Export(GoToWindowPluginConstants.GoToWindowPluginContractName, typeof(IGoToWindowPlugin))]
    public class DebugPlugin : IGoToWindowPlugin
    {
        public IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList()
        {
            return new[] {
                new DebugSearchResult(new DebugListEntry())
            };
        }
    }
}
