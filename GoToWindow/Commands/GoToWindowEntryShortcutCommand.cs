using GoToWindow.Api;
using System;
using System.Windows.Input;

namespace GoToWindow.Commands
{
    public class GoToWindowEntryShortcutCommand : ICommand
    {
        private readonly Func<int, IWindowEntry> _getEntryAt;

        public event EventHandler CanExecuteChanged;
        public event EventHandler Executed;

        public GoToWindowEntryShortcutCommand(Func<int, IWindowEntry> getEntryAt)
        {
            _getEntryAt = getEntryAt;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var selectedWindowEntry = _getEntryAt(Int32.Parse((string)parameter) - 1);

            if (selectedWindowEntry != null)
                selectedWindowEntry.Focus();

            if (Executed != null)
                Executed(this, new EventArgs());
        }
    }
}
