using System.Windows;
using GoToWindow.ViewModels;
using System.Diagnostics;

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

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
