using GoToWindow.Api;
using GoToWindow.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace GoToWindow.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static MainWindowViewModel Load()
        {
            var instance = new MainWindowViewModel();
            var viewSource = new CollectionViewSource();
            viewSource.Source = WindowsListFactory.Load().Windows;
            instance.Windows = viewSource;
            return instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public CollectionViewSource Windows { get; private set; }
        public IWindowEntry SelectedWindowEntry { get; set; }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public ICommand GoToWindowEntryShortcut { get; private set; }

        public event EventHandler Close;

        public MainWindowViewModel()
        {
            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += goToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;
        }

        private IWindowEntry GetEntryAt(int index)
        {
            var items = Windows.View.Cast<IWindowEntry>().ToArray();

            if (index < items.Length)
                return items[index];

            return null;
        }

        void goToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }

    }
}
