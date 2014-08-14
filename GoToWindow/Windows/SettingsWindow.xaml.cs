using GoToWindow.ViewModels;
using System;
using System.Windows;

namespace GoToWindow.Windows
{
    public partial class SettingsWindow : Window
    {
        private bool _originalStartWithWindowsIsChecked;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
			((SettingsViewModel)DataContext).Apply();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
