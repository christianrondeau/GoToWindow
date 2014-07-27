using System.Collections.Generic;

namespace GoToWindow.Api
{
    public class WindowsList
    {
        public WindowsList(IList<IWindowEntry> windows)
        {
            Windows = windows;
        }

        public IList<IWindowEntry> Windows { get; private set; }
    }
}
