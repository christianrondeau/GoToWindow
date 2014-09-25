using System.Windows;

namespace GoToWindow.Windows
{
	public partial class UpdateWindow : Window
	{
		public UpdateWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
