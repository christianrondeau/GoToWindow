using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.Plugins.ExplorerExtensions
{
	public class ExplorerWindowSearchResult : IWindowSearchResult
	{
		private readonly IWindowSearchResult _window;
		private readonly string _path;

		public UserControl View { get { return _window.View; } }
		public BitmapFrame Icon { get { return _window.Icon; } }
		public string Title { get { return _path; } }
		public string ProcessName { get { return _window.ProcessName; } }
		public IntPtr HWnd { get { return _window.HWnd; } }

		public bool IsShown(string searchQuery)
		{
			return SearchHelper.IsShown(searchQuery, ProcessName, _path);
		}

		public ExplorerWindowSearchResult(IWindowSearchResult window, string path)
		{
			_window = window;
			_path = path;
		}

		public void Select()
		{
			_window.Select();
		}
	}
}