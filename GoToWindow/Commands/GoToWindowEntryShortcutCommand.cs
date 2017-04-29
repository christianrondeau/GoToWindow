using System;
using System.Windows.Input;
using GoToWindow.Extensibility;

namespace GoToWindow.Commands
{
	public class GoToWindowEntryShortcutCommand : ICommand
	{
		private readonly Func<int, ISearchResult> _getEntryAt;

#pragma warning disable 67
		public event EventHandler CanExecuteChanged;
#pragma warning restore 67
		public event EventHandler Executed;

		public GoToWindowEntryShortcutCommand(Func<int, ISearchResult> getEntryAt)
		{
			_getEntryAt = getEntryAt;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			var selectedWindowEntry = _getEntryAt(int.Parse((string)parameter) - 1);

			selectedWindowEntry?.Select();

			Executed?.Invoke(this, new EventArgs());
		}
	}
}
