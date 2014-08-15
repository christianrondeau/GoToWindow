using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GoToWindow.Commands;
using GoToWindow.Extensibility;
using log4net;

namespace GoToWindow.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MainViewModel).Assembly, "GoToWindow");

        public void Load(IEnumerable<IGoToWindowPlugin> plugins)
        {
			Empty();

            var list = new List<ISearchResult>();
			var disabledPlugins = Properties.Settings.Default.DisabledPlugins ?? new StringCollection();

			foreach (var plugin in plugins.Where(plugin => !disabledPlugins.Contains(plugin.Id)))
			{
				Stopwatch stopwatch = null;

				if(Log.IsDebugEnabled)
				{
					stopwatch = new Stopwatch();
					stopwatch.Start();
				}

				plugin.BuildList(list);

				if (stopwatch != null)
				{
					stopwatch.Stop();
					Log.Debug(string.Format("Plugin '{0}' took {1} to execute, now at {2} results.", plugin.Title, stopwatch.Elapsed, list.Count));
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
			IsEmpty = true;
		}

		private string _searchText;
		private bool _isEmpty;

		public event PropertyChangedEventHandler PropertyChanged;
        public CollectionViewSource Windows { get; private set; }
        public ISearchResult SelectedWindowEntry { get; set; }

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
			private set
			{
				_isEmpty = value;
				OnPropertyChanged("IsEmpty");
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
			Windows = new CollectionViewSource();

            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += GoToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;

			Empty();
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

        void GoToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }

		public void AskClose()
		{
			if (Close != null)
				Close(this, new EventArgs());
		}
	}
}
