using System.Collections.Generic;

namespace GoToWindow.Extensibility
{
    public interface IGoToWindowPlugin
    {
        IEnumerable<IGoToWindowSearchResult> BuildInitialSearchResultList();
    }
}
