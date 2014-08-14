using GoToWindow.Api;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Chrome
{
    public class ChromeTab : TabBase, ITab
    {
        private int _tabIndex { get; set; }

        public ChromeTab(string title, int tabIndex)
			: base(title)
        {
            _tabIndex = tabIndex;
        }

        public void Select()
        {
            if (_tabIndex <= 0 || _tabIndex > 9)
                return;

            KeyboardSend.KeyDown(KeyboardSend.LCtrl);
            KeyboardSend.KeyPress(KeyboardSend.GetNumber(_tabIndex));
            KeyboardSend.KeyUp(KeyboardSend.LCtrl);
        }
    }
}
