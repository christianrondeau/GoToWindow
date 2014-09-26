using System;
using System.Windows;

namespace GoToWindow.Windows
{
	public partial class FirstRunWindow : Window
	{
		public FirstRunWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
