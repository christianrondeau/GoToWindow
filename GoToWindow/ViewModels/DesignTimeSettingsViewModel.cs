using GoToWindow.Api;
using System.Collections.Generic;

namespace GoToWindow.ViewModels
{
	public class DesignTimeSettingsViewModel : SettingsViewModel
	{	
		public DesignTimeSettingsViewModel()
		{
			ShortcutControlKey = ModifierVirtualKeys.LWin;
			ShortcutKey = /* VK_TAB */ 0x09;
			ShortcutPressesBeforeOpen = 1;

			Version = "0.0.0";
			Plugins = new List<SettingsPluginViewModel>
			{
				new SettingsPluginViewModel{Id = "plugin-1", Name="Plugin 1", Enabled=true},
				new SettingsPluginViewModel{Id = "plugin-2", Name="Plugin 2", Enabled=false}
			};
			WindowListSingleClick = true;
			NoElevatedPrivilegesWarning = true;
			LatestAvailableRelease = "9.9.9";
			UpdateAvailable = CheckForUpdatesStatus.UpdateAvailable;
		}
	}
}
