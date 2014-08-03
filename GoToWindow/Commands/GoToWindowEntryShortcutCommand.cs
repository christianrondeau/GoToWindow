﻿using System;
using System.Windows.Input;
using GoToWindow.Extensibility;

namespace GoToWindow.Commands
{
    public class GoToWindowEntryShortcutCommand : ICommand
    {
        private readonly Func<int, IGoToWindowSearchResult> _getEntryAt;

        public event EventHandler CanExecuteChanged;
        public event EventHandler Executed;

        public GoToWindowEntryShortcutCommand(Func<int, IGoToWindowSearchResult> getEntryAt)
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
                selectedWindowEntry.Select();

            if (Executed != null)
                Executed(this, new EventArgs());
        }
    }
}
