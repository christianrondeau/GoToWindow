using System.Collections.Generic;
using GoToWindow.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GoToWindow.Extensibility;

namespace GoToWindow.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public static MainViewModel Load(IEnumerable<IGoToWindowPlugin> plugins)
        {
            var list = new List<ISearchResult>();

            foreach (var plugin in plugins)
                plugin.BuildList(list);

            var instance = new MainViewModel();
            var viewSource = new CollectionViewSource
            {
                Source = list.ToArray()
            };
            instance.Windows = viewSource;
            return instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public CollectionViewSource Windows { get; private set; }
        public ISearchResult SelectedWindowEntry { get; set; }

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

        public MainViewModel()
        {
            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += GoToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;
        }

        private ISearchResult GetEntryAt(int index)
        {
            var items = Windows.View.Cast<ISearchResult>().ToArray();

            if (index < items.Length)
                return items[index];

            return null;
        }

        void GoToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }

    }
}
