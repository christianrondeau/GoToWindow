using GoToWindow.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GoToWindow.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly IGoToWindowContext _context;

        public ShowSettingsCommand(IGoToWindowContext context)
        {
            _context = context;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
			_context.ShowSettings();
        }
    }
}
