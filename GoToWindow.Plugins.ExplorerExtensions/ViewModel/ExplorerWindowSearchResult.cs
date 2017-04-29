using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.Plugins.ExplorerExtensions.ViewModel
{
	public class ExplorerWindowSearchResult : IWindowSearchResult
	{
		private readonly IWindowSearchResult _window;

		public UserControl View => _window.View;
		public BitmapFrame Icon => _window.Icon;
		public string Title { get; }

		public string ProcessName => _window.ProcessName;
		public string Error => _window.Error;
		public IntPtr HWnd => _window.HWnd;

		public bool IsVisible => _window.IsVisible;

		public bool IsShown(string searchQuery)
		{
			return SearchHelper.IsShown(searchQuery, ProcessName, Title);
		}

		public ExplorerWindowSearchResult(IWindowSearchResult window, string path)
		{
			_window = window;
			Title = path;
		}

		public void Select()
		{
			_window.Select();
		}

        public void SetError(string message)
        {
            _window.SetError(message);
        }
	}
}