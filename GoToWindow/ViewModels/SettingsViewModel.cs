using GoToWindow.Api;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoToWindow.ViewModels
{
	public class SettingsViewModel
	{
		private readonly GoToWindowContext _context;

		private bool _originalHookAltTab;
		private bool _originalStartWithWindows;

		public SettingsViewModel(GoToWindowContext context)
		{
			_context = context;

			Load();
		}

		public bool HookAltTab { get; set; }
		public bool StartWithWindows { get; set; }
		public Visibility NoElevatedPrivilegesWarning { get; set; }
		public string Version { get; set; }

		public void Load()
		{
			HookAltTab = _originalHookAltTab = Properties.Settings.Default.HookAltTab;
			StartWithWindows = _originalStartWithWindows = GetStartWithWindows();

			NoElevatedPrivilegesWarning = GetHasElevatedPrivileges()
				? Visibility.Hidden
				: Visibility.Visible;

			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		private bool GetStartWithWindows()
		{
			var runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
			if (runList != null)
			{
				var executablePath = Assembly.GetExecutingAssembly().Location;
				return ((string)runList.GetValue("GoToWindow") == executablePath);
			}

			return false;
		}

		private bool GetHasElevatedPrivileges()
		{
			if (WindowsVersion.IsWindows8())
			{
				var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(0x200);
			}

			return true;
		}

		public void Apply()
		{
			if (_originalStartWithWindows != StartWithWindows)
			{
				UpdateStartWithWindows(StartWithWindows == true);
			}

			if(_originalHookAltTab != HookAltTab)
			{
				Properties.Settings.Default.HookAltTab = HookAltTab;
				_context.EnableAltTabHook(Properties.Settings.Default.HookAltTab);
				Properties.Settings.Default.Save();
			}
		}

		private void UpdateStartWithWindows(bool active)
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
