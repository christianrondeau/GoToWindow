using System.Collections.Generic;

namespace GoToWindow.Extensibility
{
	public interface IGoToWindowPlugin
	{
		string Id { get; }
		string Title { get; }
		GoToWindowPluginSequence Sequence { get; }

		void BuildList(List<ISearchResult> list);
	}
}
