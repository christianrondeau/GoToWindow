using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GoToWindow.Commands;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;
using GoToWindow.Windows;
using log4net;

namespace GoToWindow.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedViewModelBase
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MainViewModel).Assembly, "GoToWindow");

		public CollectionViewSource Windows { get; protected set; }
		public ISearchResult SelectedWindowEntry { get; set; }

		private string _searchText;
        public string SearchText
        {
            get => _searchText;
	        set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

		private bool _isLoading;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				OnPropertyChanged("IsLoading");
			}
		}

		private int _availableWindowWidth;
		public int AvailableWindowWidth
		{
			get => _availableWindowWidth;
			set
			{
				_availableWindowWidth = value;
				OnPropertyChanged("AvailableWindowWidth");
			}
		}

		private int _availableWindowHeight;
		public int AvailableWindowHeight
		{
			get => _availableWindowHeight;
			set
			{
				_availableWindowHeight = value;
				OnPropertyChanged("AvailableWindowHeight");
			}
		}

		private bool _isRowIndexVisible;
		public bool IsRowIndexVisible
		{
			get => _isRowIndexVisible;
			set
			{
				_isRowIndexVisible = value;
				OnPropertyChanged("IsRowIndexVisible");
			}
		}

		private bool _updateAvailable;
		public bool UpdateAvailable
		{
			get => _updateAvailable;
			set
			{
				_updateAvailable = value;
				OnPropertyChanged("UpdateAvailable");
			}
		}

        public ICommand GoToWindowEntryShortcut { get; }

		public event CloseEventHandler Close;

        public MainViewModel()
        {
			Windows = new CollectionViewSource();

            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += GoToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;

	        SelectedWindowEntry = null;
	        Windows.Source = null;
	        SearchText = "";
	        IsRowIndexVisible = false;
	        IsLoading = true;
		}

		public void Load(IEnumerable<IGoToWindowPlugin> plugins)
		{
			try
			{
				var list = new List<ISearchResult>();
				var disabledPlugins = Properties.Settings.Default.DisabledPlugins ?? new StringCollection();

				foreach (var plugin in plugins.Where(plugin => !disabledPlugins.Contains(plugin.Id)))
				{
					using (new PerformanceLogger($"Plugin '{plugin.Title}'"))
					{
						plugin.BuildList(list);
					}
				}

				Windows.Source = list.ToArray();
			}
			catch (Exception exc)
			{
				Windows.Source = new ISearchResult[]
				{
					new ErrorResult("Could not load the list of windows", exc, () => new BasicListEntry())
				};
				Log.Error("Error while loading the windows list", exc);
			}

			IsLoading = false;
		}

		public void AskClose(MainWindow window, bool requested)
		{
			Close?.Invoke(window, new CloseEventArgs(requested));
		}

        private ISearchResult GetEntryAt(int index)
        {
	        var items = Windows?.View?.Cast<ISearchResult>().ToArray();

            return index < items?.Length ? items[index] : null;
        }

        private void GoToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
			Close?.Invoke(this, new CloseEventArgs(true));
		}
	}

	public delegate void CloseEventHandler(object sender, CloseEventArgs args);

	public class CloseEventArgs : EventArgs
	{
		public bool Requested { get; }

		public CloseEventArgs(bool requested)
		{
			Requested = requested;
		}
	}
}
