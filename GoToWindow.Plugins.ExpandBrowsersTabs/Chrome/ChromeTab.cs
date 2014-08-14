using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Chrome
{
    public class ChromeTab
    {
        public string Title { get; set; }
        public int TabIndex { get; set; }

        public ChromeTab(string title, int tabIndex)
        {
            Title = title;
            TabIndex = tabIndex;
        }

        public void Select()
        {
            if (TabIndex <= 0 || TabIndex > 9)
                return;

            KeyboardSend.KeyDown(KeyboardSend.LCtrl);
            KeyboardSend.KeyPress(KeyboardSend.GetNumber(TabIndex));
            KeyboardSend.KeyUp(KeyboardSend.LCtrl);
        }
    }
}
