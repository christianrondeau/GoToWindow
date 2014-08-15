using System;
using System.Threading;
using GoToWindow.Api;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

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
            const int maxShortcutNumber = 8;

			if (_tabIndex <= 0)
				return;

			KeyboardSend.KeyDown(KeyboardSend.LCtrl);
		    
		    if (_tabIndex <= maxShortcutNumber)
		    {
		        KeyboardSend.KeyPress(KeyboardSend.GetNumber(_tabIndex));
		    }
		    else
		    {
                KeyboardSend.KeyPress(KeyboardSend.GetNumber(maxShortcutNumber));

		        for (var i = 0; i < _tabIndex - maxShortcutNumber; i++)
		        {
		            const int timeToDigestPreviousKeyPress = 10;
		            Thread.Sleep(timeToDigestPreviousKeyPress);
		            KeyboardSend.KeyPress(KeyboardSend.Tab);
		        }
		    }
		    KeyboardSend.KeyUp(KeyboardSend.LCtrl);
		}
	}
}
