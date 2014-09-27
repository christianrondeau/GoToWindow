using System;
using System.ComponentModel;
using System.Windows;
using GoToWindow.Squirrel;
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

		private void Window_Initialized(object sender, EventArgs e)
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
			SquirrelUpdater.ShowUpdateWindow();
			Close();
			e.Handled = true;
		}

		private void OfficialWebsite_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (DataContext != null && !((SettingsViewModel)DataContext).Enabled)
				e.Cancel = true;
		}
	}
}
