using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = "add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /t REG_SZ /d \"" + executablePath + "\" /f",
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                    
                };
                process.Start();
                process.WaitForExit();
            }
            else
            {
                var executablePath = Assembly.GetExecutingAssembly().Location;

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = "delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" /v \"GoToWindow\" /f",
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
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
            RegistryKey runList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            var executablePath = Assembly.GetExecutingAssembly().Location;
            startWithWindowsCheckbox.IsChecked = _originalStartWithWindowsIsChecked = ((string)runList.GetValue("GoToWindow") == executablePath);

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            noElevatedPrivilegesWarning.Visibility = (principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(0x200)) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
