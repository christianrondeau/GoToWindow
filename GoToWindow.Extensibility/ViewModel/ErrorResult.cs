using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility.ViewModel
{
	public class ErrorResult : SearchResultBase, IBasicSearchResult, INotifyPropertyChanged
	{
		private readonly string _message;
		private readonly Exception _exc;

		public event PropertyChangedEventHandler PropertyChanged;

		public ErrorResult(string message, Exception exc, Func<UserControl> viewCtor)
			: base(viewCtor)
		{
			_message = message;
			_exc = exc;
		}

		public void Select()
		{
		}

		public bool IsShown(string searchQuery)
		{
			return true;
		}

		public BitmapFrame Icon
		{
			get { return null; }
		}

		public string Title
		{
			get { return _message; }
		}

		public string ProcessName
		{
			get { return "error"; }
		}

		public string Error
		{
			get { return _exc.Message; }
		}
	}
}