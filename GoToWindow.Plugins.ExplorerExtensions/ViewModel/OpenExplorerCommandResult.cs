using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;
using log4net;

namespace GoToWindow.Plugins.ExplorerExtensions.ViewModel
{
	public class OpenExplorerCommandResult : SearchResultBase, IBasicCommandResult, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OpenExplorerCommandResult).Assembly, "GoToWindow");

		private string _text;
		private string _query;

		public event PropertyChangedEventHandler PropertyChanged;

		public string BeforeText { get { return "Open Windows Explorer with path '"; } }
		public string AfterText { get { return "'"; } }

		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				OnPropertyChanged("Text");
			}
		}

		public OpenExplorerCommandResult()
			: base(() => new BasicCommandEntry())
		{
			
		}

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public void Select()
		{
			Log.DebugFormat("Launching Explorer with '{0}'.", _query);
			Process.Start(_query);
		}

		public bool IsShown(string searchQuery)
		{
			_query = searchQuery;
			Text = _query;
			var isPathQuery = !String.IsNullOrWhiteSpace(searchQuery) && searchQuery.Length >= 2 && searchQuery[1] == ':';

			if (!isPathQuery) return false;

			return Directory.Exists(searchQuery);
		}
	}
}