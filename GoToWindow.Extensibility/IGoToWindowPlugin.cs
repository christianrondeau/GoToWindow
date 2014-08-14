using System.Collections.Generic;

namespace GoToWindow.Extensibility
{
    public interface IGoToWindowPlugin
    {
        GoToWindowPluginSequence Sequence { get; }

        void BuildList(List<ISearchResult> list);
    }
}
