using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GoToWindow.Commands;
using GoToWindow.Extensibility;

namespace GoToWindow.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedViewModelBase
	{
		public CollectionViewSource Windows { get; protected set; }
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

		private bool _isEmpty;
		public bool IsEmpty
		{
			get { return _isEmpty; }
			set
			{
				_isEmpty = value;
				OnPropertyChanged("IsEmpty");
			}
		}

		private int _availableWindowWidth;
		public int AvailableWindowWidth
		{
			get { return _availableWindowWidth; }
			set
			{
				_availableWindowWidth = value;
				OnPropertyChanged("AvailableWindowWidth");
			}
		}

		private int _availableWindowHeight;
		public int AvailableWindowHeight
		{
			get { return _availableWindowHeight; }
			set
			{
				_availableWindowHeight = value;
				OnPropertyChanged("AvailableWindowHeight");
			}
		}

		private bool _isRowIndexVisible;
		public bool IsRowIndexVisible
		{
			get { return _isRowIndexVisible; }
			set
			{
				_isRowIndexVisible = value;
				OnPropertyChanged("IsRowIndexVisible");
			}
		}

		private bool _updateAvailable;
		public bool UpdateAvailable
		{
			get { return _updateAvailable; }
			set
			{
				_updateAvailable = value;
				OnPropertyChanged("UpdateAvailable");
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
