using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Controls;

namespace GoToWindow.Extensibility.ViewModel
{
    public abstract class SearchResultBase
    {
        private readonly Func<UserControl> _viewCtor;
        protected UserControl _view;

        public UserControl View
        {
            get { return _view ?? (_view = _viewCtor()); }
        }

        public SearchResultBase(Func<UserControl> viewCtor)
        {
            _viewCtor = viewCtor;
        }

        protected static bool IsShown(string searchQuery, params string[] fields)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return true;

            var text = String.Join(" ", fields);

            return searchQuery
                .Split(' ')
                .All(word => CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, word, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) > -1);
        }
    }
}
