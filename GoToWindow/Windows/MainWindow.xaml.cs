using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using GoToWindow.Api;
using GoToWindow.Extensibility;
using GoToWindow.ViewModels;
using log4net;

namespace GoToWindow.Windows
{
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow).Assembly, "GoToWindow");

        private const int PageCount = 4;

        private bool _releasedAlt;
        private bool _closeOnAltUp;

        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void TabAgain()
        {
            if (!_releasedAlt)
                _closeOnAltUp = true;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                ScrollToPreviousWindowEntry(1);
            else
                ScrollToNextWindowEntry(1);
        }

		public void SetFocus()
		{
			Activate();
			SearchTextBox.Focus();
		}

        void ViewModel_Close(object sender, EventArgs e)
        {
			ViewModel.AskClose();
        }

        private void WindowsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FocusSelectedWindowItem();
        }

        private void FocusSelectedWindowItem()
        {
            var windowEntry = WindowsListView.SelectedItem as ISearchResult;
            if (windowEntry != null)
            {
                windowEntry.Select();

                ViewModel.AskClose();

                return;
            }
            
            var searchQuery = SearchTextBox.Text;
            if(!String.IsNullOrWhiteSpace(searchQuery))
            {
                Log.DebugFormat("Launching Windows Search with '{0}'.", searchQuery);

                WindowsSearch.Launch(searchQuery);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
				ViewModel.AskClose();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel.Windows.View == null)
                return;

            ApplyFilter(SearchTextBox.Text);

            if (WindowsListView.SelectedIndex == -1 && WindowsListView.Items.Count > 0)
                WindowsListView.SelectedIndex = 0;
        }

		public void DataReady()
		{
		    _releasedAlt = false;
		    _closeOnAltUp = false;

			ApplyFilter("");

			if (WindowsListView.Items.Count > 1)
				WindowsListView.SelectedIndex = 1;
			else if (WindowsListView.Items.Count > 0)
				WindowsListView.SelectedIndex = 0;
		}

        private void ApplyFilter(string searchQuery)
        {
			if (ViewModel.Windows.View == null) return;

            ViewModel.Windows.View.Filter = item => SearchFilter((ISearchResult)item, searchQuery);
        }

        private static bool SearchFilter(ISearchResult window, string text)
        {
            return window.IsShown(text);
        }

        private void SearchBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Focus();
            SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    FocusSelectedWindowItem();
                    break;

                case Key.Down:
                    ScrollToNextWindowEntry(1);
                    break;

                case Key.Up:
                    ScrollToPreviousWindowEntry(1);
                    break;

                case Key.PageDown:
                    ScrollToNextWindowEntry(PageCount);
                    break;

                case Key.PageUp:
                    ScrollToPreviousWindowEntry(PageCount);
                    break;

                default:
                    return;
            }

            e.Handled = true;
        }

        private void ScrollToPreviousWindowEntry(int count)
        {
            const int minIndex = 0;

            if (WindowsListView.SelectedIndex <= 0)
            {
                WindowsListView.SelectedIndex = WindowsListView.Items.Count - 1;
            }
            else
            {
                var newIndex = WindowsListView.SelectedIndex - count;

                WindowsListView.SelectedIndex = newIndex <= minIndex ? minIndex : newIndex;
            }
            WindowsListView.ScrollIntoView(WindowsListView.SelectedItem);
        }

        private void ScrollToNextWindowEntry(int count)
        {
            var maxIndex = WindowsListView.Items.Count - 1;

            if (WindowsListView.SelectedIndex >= maxIndex)
            {
                WindowsListView.SelectedIndex = 0;
            }
            else
            {
                var newIndex = WindowsListView.SelectedIndex + count;

                WindowsListView.SelectedIndex = newIndex >= maxIndex ? maxIndex : newIndex;
            }
            WindowsListView.ScrollIntoView(WindowsListView.SelectedItem);
        }

        private void WindowsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusSelectedWindowItem();
                e.Handled = true;
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
			ViewModel.AskClose();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Log.Debug("Window activated.");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Log.Debug("Window deactivated.");
			ViewModel.AskClose();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.System && e.SystemKey == Key.LeftAlt)
            {
                if (_closeOnAltUp)
                    FocusSelectedWindowItem();

                _releasedAlt = true;
            }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowControlMenuDisabler.DisableControlMenu(
                new WindowInteropHelper(this).Handle
                );
        }
    }
}
