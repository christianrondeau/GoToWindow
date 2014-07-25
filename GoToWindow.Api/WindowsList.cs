using System.Collections.Generic;

namespace GoToWindow.Api
{
    public class WindowsList
    {
        public WindowsList(IList<IWindow> windows)
        {
            Windows = windows;
        }

        public IList<IWindow> Windows { get; private set; }
    }
}
