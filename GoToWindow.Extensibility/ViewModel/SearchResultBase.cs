using System;
using System.Windows.Controls;

namespace GoToWindow.Extensibility.ViewModel
{
	public abstract class SearchResultBase
	{
		private readonly Func<UserControl> _viewCtor;
	    private UserControl _view;

		public UserControl View
		{
			get { return _view ?? (_view = _viewCtor()); }
		}

	    protected SearchResultBase(Func<UserControl> viewCtor)
		{
			_viewCtor = viewCtor;
		}

		protected static bool IsShown(string searchQuery, params string[] fields)
		{
			return SearchHelper.IsShown(searchQuery, fields);
		}
	}
}
