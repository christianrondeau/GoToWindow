using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Extensibility
{
    public enum GoToWindowPluginSequence
    {
        BeforeCore = 512,
        Core = 1024,
        AfterCore = 2048
    }
}
