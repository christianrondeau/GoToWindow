using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GoToWindow.Extensibility.ViewModel
{
	public class ErrorResult : SearchResultBase, IBasicSearchResult, INotifyPropertyChanged
	{
		private readonly Exception _exc;

#pragma warning disable 67
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

		public ErrorResult(string message, Exception exc, Func<UserControl> viewCtor)
			: base(viewCtor)
		{
			Title = message;
			_exc = exc;
		}

		public void Select()
		{
		}

		public bool IsShown(string searchQuery)
		{
			return true;
		}

		public BitmapFrame Icon => null;

		public string Title { get; }

		public string ProcessName => "error";

		public string Error => _exc.Message;
	}
}