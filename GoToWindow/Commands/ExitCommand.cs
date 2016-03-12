using System;
using System.Windows;
using System.Windows.Input;

namespace GoToWindow.Commands
{
	public class ExitCommand : ICommand
	{
#pragma warning disable 67
		public event EventHandler CanExecuteChanged;
#pragma warning restore 67

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			Application.Current.Shutdown();
		}
	}
}
