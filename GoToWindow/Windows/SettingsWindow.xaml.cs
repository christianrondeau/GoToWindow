using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace GoToWindow.Windows
{
    public partial class SettingsWindow : Window
    {
        private bool _originalStartWithWindowsIsChecked;
        private readonly IGoToWindowContext _context;

        public SettingsWindow(IGoToWindowContext context)
        {
            _context = context;

            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();

            if(_originalStartWithWindowsIsChecked != startWithWindowsCheckbox.IsChecked)
            {
                UpdateStartWithWindows(startWithWindowsCheckbox.IsChecked == true);
            }

            _context.EnableAltTabHook(Properties.Settings.Default.HookAltTab);

            Close();
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

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (runList != null)
            {
                var executablePath = Assembly.GetExecutingAssembly().Location;
                startWithWindowsCheckbox.IsChecked = _originalStartWithWindowsIsChecked = ((string) runList.GetValue("GoToWindow") == executablePath);
            }

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            noElevatedPrivilegesWarning.Visibility = (principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(0x200)) ? Visibility.Hidden : Visibility.Visible;

            versionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
