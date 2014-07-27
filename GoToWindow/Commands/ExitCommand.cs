using System;
using System.Windows;
using System.Windows.Input;

namespace GoToWindow
{
    public class ExitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ExitCommand()
        {
        }

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
