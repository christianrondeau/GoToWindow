using System;
using System.Windows.Input;

namespace GoToWindow.Commands
{
	public class OpenMainWindowCommand : ICommand
	{
		private readonly IGoToWindowContext _context;

#pragma warning disable 67
		public event EventHandler CanExecuteChanged;
#pragma warning restore 67

		public OpenMainWindowCommand(IGoToWindowContext context)
		{
			_context = context;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			_context.Show();
		}
	}
}
