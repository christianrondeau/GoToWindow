using GoToWindow.Api;
using GoToWindow.Properties;

namespace GoToWindow.ViewModels
{
	public class FirstRunViewModel
	{
		public string ShortcutDescription
		{
			get
			{
				var shortcut = KeyboardShortcut.FromString(Settings.Default.OpenShortcut);
				return shortcut.ToHumanReadableString();
			}
		}	
	}
}