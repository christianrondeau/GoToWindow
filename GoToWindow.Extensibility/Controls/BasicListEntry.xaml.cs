using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoToWindow.Extensibility.Controls
{
	public partial class BasicListEntry : UserControl
	{
		private Window _window;

		public BasicListEntry()
		{
			InitializeComponent();
		}
		/*
		private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.LeftCtrl)
				RowIndexLabel.Visibility = Visibility.Visible;
		}

		private void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.LeftCtrl)
				RowIndexLabel.Visibility = Visibility.Hidden;
		}
		*/
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{/*
			_window = Window.GetWindow(this);
			_window.PreviewKeyDown += Window_PreviewKeyDown;
			_window.PreviewKeyUp += Window_PreviewKeyUp;*/
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{/*
			if(_window != null)
			{
				_window.PreviewKeyDown -= Window_PreviewKeyDown;
				_window.PreviewKeyUp -= Window_PreviewKeyUp;
			}*/
		}
	}
}
