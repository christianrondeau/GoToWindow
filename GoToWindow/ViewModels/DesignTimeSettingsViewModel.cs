using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using GoToWindow.Api;
using Microsoft.Win32;

namespace GoToWindow.ViewModels
{
	public class DesignTimeSettingsViewModel : SettingsViewModel
	{
		public DesignTimeSettingsViewModel()
			: base()
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
		}
	}
}
