using System.ComponentModel;
using System.Text.RegularExpressions;
using GoToWindow.Api;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;
using log4net;

namespace GoToWindow.Plugins.Core.ViewModel
{
	public class WindowSearchCommandResult : SearchResultBase, IBasicCommandResult, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(WindowSearchCommandResult).Assembly, "GoToWindow");
		private static readonly Regex ValidQuery = new Regex(@"^[^:\\/<>|""]+$", RegexOptions.Compiled);

		private string _text;
		private string _query;

		public event PropertyChangedEventHandler PropertyChanged;

		public string BeforeText => "Search for '";
		public string AfterText => "' using Windows Search";

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		public WindowSearchCommandResult()
			: base(() => new BasicCommandEntry())
		{
			
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void Select()
		{
			Log.DebugFormat("Launching Windows Search with '{0}'.", _query);
			WindowsSearch.Launch(_query);
		}

		public bool IsShown(string searchQuery)
		{
			_query = searchQuery;
			Text = _query;
			return !string.IsNullOrWhiteSpace(searchQuery) && ValidQuery.IsMatch(searchQuery);
		}
	}
}