using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public interface ITab
	{
		string Title { get; set; }
		void Select();
	}
}
