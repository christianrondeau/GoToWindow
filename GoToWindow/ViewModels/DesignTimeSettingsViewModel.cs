using System.Collections.Generic;
using GoToWindow.Squirrel;

namespace GoToWindow.ViewModels
{
	public class DesignTimeSettingsViewModel : SettingsViewModel
	{
		public DesignTimeSettingsViewModel()
		{
			Version = "0.0.0";
			Plugins = new List<SettingsPluginViewModel>
			{
				new SettingsPluginViewModel{Id = "plugin-1", Name="Plugin 1", Enabled=true},
				new SettingsPluginViewModel{Id = "plugin-2", Name="Plugin 2", Enabled=false}
			};
			ShortcutPressesBeforeOpen = 2;
			WindowListSingleClick = true;
			HookAltTab = true;
			NoElevatedPrivilegesWarning = true;
			LatestAvailableRelease = "9.9.9";
			UpdateAvailable = CheckForUpdatesStatus.UpdateAvailable;
		}
	}
}
