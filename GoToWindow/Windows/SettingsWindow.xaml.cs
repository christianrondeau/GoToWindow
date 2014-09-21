using System;
using System.Windows;
using GoToWindow.ViewModels;
using System.Diagnostics;
using System.Windows.Navigation;

namespace GoToWindow.Windows
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			HelpWebBrowser.NavigateToString(Properties.Resources.Help);
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

		private void UpdateNow_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			((SettingsViewModel)DataContext).Update();
			e.Handled = true;
		}

		private void OfficialWebsite_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!((SettingsViewModel)DataContext).Enabled)
				e.Cancel = true;
		}
	}
}
