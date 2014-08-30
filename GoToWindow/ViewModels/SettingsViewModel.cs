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
	public class SettingsViewModel
	{
		private readonly IGoToWindowContext _context;

		private bool _originalHookAltTab;
		private bool _originalStartWithWindows;

	    public SettingsViewModel(IGoToWindowContext context)
		{
			_context = context;

			Load();
		}

		public bool HookAltTab { get; set; }
		public bool StartWithWindows { get; set; }
        public int ShortcutPressesBeforeOpen { get; set; }
        public bool WindowListSingleClick { get; set; }
		public Visibility NoElevatedPrivilegesWarning { get; set; }
		public string Version { get; set; }
		public List<SettingsPluginViewModel> Plugins { get; private set; }

		public void Load()
		{
			HookAltTab = _originalHookAltTab = Properties.Settings.Default.HookAltTab;
			StartWithWindows = _originalStartWithWindows = GetStartWithWindows();
			ShortcutPressesBeforeOpen = Properties.Settings.Default.ShortcutPressesBeforeOpen;
		    WindowListSingleClick = Properties.Settings.Default.WindowListSingleClick;

			NoElevatedPrivilegesWarning = WindowsRuntimeHelper.GetHasElevatedPrivileges()
				? Visibility.Hidden
				: Visibility.Visible;

			var disabledPlugins = Properties.Settings.Default.DisabledPlugins ?? new StringCollection();

			Plugins = _context.PluginsContainer.Plugins
				.Select(plugin => new SettingsPluginViewModel
					{
						Id = plugin.Id,
						Enabled = !disabledPlugins.Contains(plugin.Id),
						Name = plugin.Title
					})
				.OrderBy(plugin => plugin.Name)
				.ToList();

			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		private static bool GetStartWithWindows()
		{
			var runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
		    
            if (runList == null) return false;
		    
            var executablePath = Assembly.GetExecutingAssembly().Location;
		    return ((string)runList.GetValue("GoToWindow") == executablePath);
		}

		public void Apply()
		{
			if (_originalStartWithWindows != StartWithWindows)
			{
				UpdateStartWithWindows(StartWithWindows);
            }

            Properties.Settings.Default.HookAltTab = HookAltTab;
            Properties.Settings.Default.ShortcutPressesBeforeOpen = ShortcutPressesBeforeOpen;
            Properties.Settings.Default.WindowListSingleClick = WindowListSingleClick;

			if(_originalHookAltTab != HookAltTab)
			{
				_context.EnableAltTabHook(HookAltTab, ShortcutPressesBeforeOpen);
			}

			var disabledPlugins = new StringCollection();
			disabledPlugins.AddRange(Plugins.Where(plugin => !plugin.Enabled).Select(plugin => plugin.Id).ToArray());
			Properties.Settings.Default.DisabledPlugins = disabledPlugins;

			Properties.Settings.Default.Save();
		}

		private static void UpdateStartWithWindows(bool active)
		{
			if (active)
			{
				var executablePath = Assembly.GetExecutingAssembly().Location;

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "reg.exe",
						Arguments = string.Format("add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /t REG_SZ /d \"{0}\" /f", executablePath),
						Verb = "runas",
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden

					}
				};
				process.Start();
				process.WaitForExit();
			}
			else
			{
				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "reg.exe",
						Arguments = "delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /f",
						Verb = "runas",
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden
					}
				};
				process.Start();
				process.WaitForExit();
			}
		}
	}
}
