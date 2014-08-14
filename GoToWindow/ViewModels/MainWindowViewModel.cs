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
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static MainWindowViewModel Load(IEnumerable<IGoToWindowPlugin> plugins)
        {
            var list = new List<IGoToWindowSearchResult>();

            foreach (var plugin in plugins)
                list.AddRange(plugin.BuildInitialSearchResultList());

            var instance = new MainWindowViewModel();
            var viewSource = new CollectionViewSource
            {
                Source = list.ToArray()
            };
            instance.Windows = viewSource;
            return instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public CollectionViewSource Windows { get; private set; }
        public IGoToWindowSearchResult SelectedWindowEntry { get; set; }

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
            goToWindowEntryShortcutCommand.Executed += GoToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;
        }

        private IGoToWindowSearchResult GetEntryAt(int index)
        {
            var items = Windows.View.Cast<IGoToWindowSearchResult>().ToArray();

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
