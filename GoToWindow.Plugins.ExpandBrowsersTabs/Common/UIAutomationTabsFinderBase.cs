using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Common
{
    // ReSharper disable once InconsistentNaming
    public abstract class UIAutomationTabsFinderBase
    {
        public bool CanGetTabsOfWindow(IWindowSearchResult item, out string errorMessage)
        {
            if (!item.IsVisible)
            {
                errorMessage = "Cannot get tabs because window is minimized";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
