using System.ComponentModel;
using System.Windows.Controls;
using GoToWindow.Extensibility;
using log4net;

namespace GoToWindow.Plugins.Debug.ViewModel
{
    public class DebugSearchResult : ISearchResult, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugSearchResult).Assembly, "GoToWindow");

		public UserControl View { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

		public DebugSearchResult(UserControl view)
		{
			View = view;
			Message = "Debug";
		}

		public void Select()
		{
			Log.Debug("Debug: Selected");
		}

		public bool IsShown(string searchQuery)
		{
			var visible = searchQuery != null && searchQuery.StartsWith(":");
		    Message = searchQuery;
		    return visible;
		}

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
	}
}