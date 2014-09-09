using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GoToWindow.Commands;
using GoToWindow.Extensibility;

namespace GoToWindow.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private string _searchText;
		private bool _isEmpty;
	    private int _availableWindowWidth;
	    private int _availableWindowHeight;
		private bool _isRowIndexVisible;

		public event PropertyChangedEventHandler PropertyChanged;

		public CollectionViewSource Windows { get; protected set; }
		public ISearchResult SelectedWindowEntry { get; set; }
		public ISearchResult CommandResult { get; private set; }
		public UserControl CommandView { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set
			{
				_isEmpty = value;
				OnPropertyChanged("IsEmpty");
			}
		}

		public int AvailableWindowWidth
		{
			get { return _availableWindowWidth; }
			set
			{
				_availableWindowWidth = value;
				OnPropertyChanged("AvailableWindowWidth");
			}
		}

		public int AvailableWindowHeight
		{
			get { return _availableWindowHeight; }
			set
			{
				_availableWindowHeight = value;
				OnPropertyChanged("AvailableWindowHeight");
			}
		}

		public bool IsRowIndexVisible
		{
			get { return _isRowIndexVisible; }
			set
			{
				_isRowIndexVisible = value;
				OnPropertyChanged("IsRowIndexVisible");
			}
		}

        public ICommand GoToWindowEntryShortcut { get; private set; }

        public event EventHandler Close;

        public MainViewModel()
        {
			Windows = new CollectionViewSource();

            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += GoToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;

			Empty();
			IsEmpty = true;
		}

		public void Load(IEnumerable<IGoToWindowPlugin> plugins)
		{
			Empty();

			var list = new List<ISearchResult>();
			var disabledPlugins = Properties.Settings.Default.DisabledPlugins ?? new StringCollection();

			foreach (var plugin in plugins.Where(plugin => !disabledPlugins.Contains(plugin.Id)))
			{
				using (new PerformanceLogger(string.Format("Plugin '{0}'", plugin.Title)))
				{
					plugin.BuildList(list);
				}
			}

			Windows.Source = list.ToArray();

			IsEmpty = false;
		}

		public void Empty()
		{
			SelectedWindowEntry = null;
			Windows.Source = null;
			SearchText = "";
			IsRowIndexVisible = false;
		}

		public void AskClose()
		{
			if (Close != null)
				Close(this, new EventArgs());
		}

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

        private ISearchResult GetEntryAt(int index)
        {
			if (Windows == null)
				return null;

            var items = Windows.View.Cast<ISearchResult>().ToArray();

            if (index < items.Length)
                return items[index];

            return null;
        }

        private void GoToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }
	}
}
