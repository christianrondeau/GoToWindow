using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GoToWindow.Api;
using GoToWindow.Extensibility;
using GoToWindow.ViewModels;
using System.Windows.Threading;
using System.Windows.Interop;
using log4net;

namespace GoToWindow.Windows
{
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow).Assembly, "GoToWindow");

        private const int PageCount = 4;

        private bool _isClosing;
        private bool _releasedAlt;
        private bool _closeOnAltUp;

        private MainWindowViewModel ViewModel { get { return (MainWindowViewModel)DataContext; } }

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

        void ViewModel_Close(object sender, EventArgs e)
        {
            if (!_isClosing)
                Close();
        }

        private void WindowsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FocusSelectedWindowItem();
        }

        private void FocusSelectedWindowItem()
        {
            var windowEntry = WindowsListView.SelectedItem as IGoToWindowSearchResult;
            if (windowEntry != null)
            {
                windowEntry.Select();

                if (!_isClosing)
                    Close();

                return;
            }
            
            var searchQuery = SearchTextBox.Text;
            if(!String.IsNullOrWhiteSpace(searchQuery))
            {
                Log.Debug(string.Format("Launching Windows Search with '{0}'.", searchQuery));

                WindowsSearch.Launch(searchQuery);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_isClosing) return;

            if (e.Key == Key.Escape)
                Close();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel == null)
                return;

            ApplyFilter(SearchTextBox.Text);

            if (WindowsListView.SelectedIndex == -1 && WindowsListView.Items.Count > 0)
                WindowsListView.SelectedIndex = 0;
        }

        private void ApplyFilter(string searchQuery)
        {
            ViewModel.Windows.View.Filter = item => SearchFilter((IGoToWindowSearchResult)item, searchQuery);
        }

        private static bool SearchFilter(IGoToWindowSearchResult window, string text)
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
            if (_isClosing) return;

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
                return;
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isClosing)
                Close();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ApplyFilter(SearchTextBox.Text);
        }

        private void WindowsListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EventHandler eventHandler = null;
            eventHandler = delegate
            {
                if (WindowsListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
                WindowsListView.ItemContainerGenerator.StatusChanged -= eventHandler;

                if (WindowsListView.Items.Count > 0)
                    WindowsListView.SelectedIndex = 0;
            };
            WindowsListView.ItemContainerGenerator.StatusChanged += eventHandler;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            var interopHelper = new WindowInteropHelper(this);
            var thisEntry = WindowEntryFactory.Create(interopHelper.Handle);

            if (!thisEntry.HasFocus())
            {
                Log.Debug("Window does not have focus when shown. Forcing focus.");
                thisEntry.Focus();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Log.Debug("Window activated.");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                Log.Debug("Window deactivated.");
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
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
    }
}
