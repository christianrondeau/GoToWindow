using System.Collections.Generic;

namespace GoToWindow.Extensibility
{
    public interface IGoToWindowPlugin
    {
        IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList();
    }

    public static class GoToWindowPluginConstants
    {
        public const string GoToWindowPluginContractName = "GoToWindow.Plugin";
    }
}
