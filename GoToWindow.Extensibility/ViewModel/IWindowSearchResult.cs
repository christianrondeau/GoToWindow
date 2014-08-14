using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Extensibility.ViewModel
{
    public interface IWindowSearchResult : ISearchResult, IBasicSearchResult
    {
        IntPtr HWnd { get; }
    }
}
