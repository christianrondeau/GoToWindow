using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public interface ITabsFinder
	{
		IEnumerable<ITab> GetTabsOfWindow(IntPtr hWnd);
	}
}
