using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
