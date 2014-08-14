using System.Collections.Generic;
using System.Linq;
using GoToWindow.Extensibility;
using GoToWindow.Plugins.ExpandBrowsersTabs.ViewModel;
using System.ComponentModel.Composition;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Extensibility.Controls;

namespace GoToWindow.Plugins.ExpandBrowsersTabs
{
    [Export(GoToWindowConstants.PluginContractName, typeof(IGoToWindowPlugin))]
    public class ExpandBrowsersTabsPlugin : IGoToWindowPlugin
    {
        public GoToWindowPluginSequence Sequence { get { return GoToWindowPluginSequence.AfterCore; } }

        public void BuildList(List<ISearchResult> list)
        {
            for(int index = list.Count - 1; index >= 0; index--)
            {
                var item = list[index] as IBasicSearchResult;

                if(item != null && item.Process == "chrome")
                {
                    list.RemoveAt(index);
                    list.Insert(index, new ChromeTabSearchResult(item, () => new BasicListEntry()));
                }
            }
        }
    }
}
