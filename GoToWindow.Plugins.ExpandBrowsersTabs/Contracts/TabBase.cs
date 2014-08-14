using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public abstract class TabBase
	{
        public string Title { get; set; }

		public TabBase(string title)
        {
            Title = title;
        }
	}
}
