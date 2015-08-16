using System;
using System.Windows;
using System.Windows.Input;

namespace GoToWindow.FakeApp
{
	public partial class MainWindow
	{
		private const int MaxKeyInputMessages = 20;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
		    var args = Environment.GetCommandLineArgs();

		    Title = args.Length >= 2 ? args[1] : "Go To Window - Fake Application (For Testing)";
		}

	    private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			InsertKeyMessage("Down: " + GetKeyEventString(e));
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			InsertKeyMessage("Up:   " + GetKeyEventString(e));
		}

		private void InsertKeyMessage(string msg)
		{
			KeyInputsListBox.Items.Insert(0, msg);
			while (KeyInputsListBox.Items.Count > MaxKeyInputMessages)
				KeyInputsListBox.Items.RemoveAt(KeyInputsListBox.Items.Count - 1);
		}

		private static string GetKeyEventString(KeyEventArgs e)
		{
			if (e.Key == Key.System)
				return e.SystemKey + " (System)";

			return e.Key.ToString();
		}

		private void Menu_Crash(object sender, RoutedEventArgs e)
		{
			throw new ApplicationException("This is an exception manually thrown for debugging purposes");
		}

		private void Menu_MessageBox(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("This is a dialog");
		}

		private void Menu_Close(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			KeyInputsListBox.Items.Clear();
		}
	}
}
