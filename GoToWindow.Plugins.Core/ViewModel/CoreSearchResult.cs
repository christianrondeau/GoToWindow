using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;
using GoToWindow.Api;
using GoToWindow.Plugins.Core.Utils;
using GoToWindow.Extensibility;
using System.Windows.Controls;
using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.Plugins.Core.ViewModel
{
    public class CoreSearchResult : SearchResultBase, IWindowSearchResult
    {
        private readonly IWindowEntry _entry;
        private BitmapFrame _icon;

        public BitmapFrame Icon { get { return _icon ?? (_icon = LoadIcon()); } }
        public string Title { get { return _entry.Title; } }
        public string Process { get { return _entry.ProcessName; } }
        public IntPtr HWnd { get { return _entry.HWnd; } }

        public CoreSearchResult(IWindowEntry entry, Func<UserControl> viewCtor)
            : base(viewCtor)
        {
            _entry = entry;
        }

        public void Select()
        {
            _entry.Focus();
        }

        public bool IsShown(string searchQuery)
        {
            return IsShown(searchQuery, Process, Title);
        }

        private BitmapFrame LoadIcon()
        {
            return IconLoader.LoadIcon(_entry.IconHandle, _entry.Executable);
        }
    }
}