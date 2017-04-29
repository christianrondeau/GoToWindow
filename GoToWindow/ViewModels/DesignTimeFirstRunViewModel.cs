using GoToWindow.Api;

namespace GoToWindow.ViewModels
{
	public class DesignTimeFirstRunViewModel
	{
		public string ShortcutDescription => KeyboardShortcut.FromString(Properties.Settings.Default.OpenShortcut).ToHumanReadableString();
	}
}