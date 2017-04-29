using System.Threading;
using GoToWindow.Api;
using GoToWindow.Plugins.ExpandBrowsersTabs.Contracts;

namespace GoToWindow.Plugins.ExpandBrowsersTabs.Common
{
	public abstract class ShortcutBasedTab : TabBase
	{
		private int TabIndex { get; }

		protected ShortcutBasedTab(string title, int tabIndex)
			: base(title)
		{
			TabIndex = tabIndex;
		}

		public void Select()
		{
			const int maxShortcutNumber = 8;

			if (TabIndex <= 0)
				return;

			KeyboardSend.KeyDown(KeyboardSend.LCtrl);

			if (TabIndex <= maxShortcutNumber)
			{
				KeyboardSend.KeyPress(KeyboardSend.GetNumber(TabIndex));
			}
			else
			{
				KeyboardSend.KeyPress(KeyboardSend.GetNumber(maxShortcutNumber));

				for (var i = 0; i < TabIndex - maxShortcutNumber; i++)
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
