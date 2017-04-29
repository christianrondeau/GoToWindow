using System;
using System.Windows;

namespace GoToWindow.Windows
{
	public partial class FirstRunWindow
	{
		public FirstRunWindow()
		{
			InitializeComponent();
		}

		public FirstRunWindow(IGoToWindowContext context)
			: this()
		{
			context.Showing += context_Showing;
		}

		private void context_Showing(object sender, EventArgs e)
		{
			Close();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
