using GoToWindow.Windows;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoToWindow.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private IGoToWindowContext _context;

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
            if (Application.Current.Windows.OfType<SettingsWindow>().Count() > 0)
                return;

            new SettingsWindow(_context).ShowDialog();
        }
    }
}
