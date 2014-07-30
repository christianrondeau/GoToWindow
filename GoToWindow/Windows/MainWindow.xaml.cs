using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GoToWindow.Api;
using GoToWindow.ViewModels;

namespace GoToWindow.Windows
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private bool _isClosing;
        private bool _releasedAlt;
        private bool _closeOnAltUp;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void TabAgain()
        {
            if (!_releasedAlt)
                _closeOnAltUp = true;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                ScrollToPreviousWindowEntry();
            else
                ScrollToNextWindowEntry();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _viewModel = MainWindowViewModel.Load();
            _viewModel.Close += ViewModel_Close;
            DataContext = _viewModel;
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
            var windowEntry = WindowsListView.SelectedItem as IWindowEntry;
            if (windowEntry != null)
            {
                if (!windowEntry.Focus())
                    MessageBox.Show("Could not show window. Try running with elevated privileges (Run as Administrator)", "Could Not Show Window", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                if (!_isClosing)
                    Close();
            }
            else if(!String.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                WindowsSearch.Launch(SearchTextBox.Text);
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
            _viewModel.Windows.View.Filter = item => SearchFilter((IWindowEntry)item, SearchTextBox.Text);

            if (WindowsListView.SelectedIndex == -1 && WindowsListView.Items.Count > 0)
                WindowsListView.SelectedIndex = 0;
        }

        private bool SearchFilter(IWindowEntry window, string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return StringContains(window.ProcessName + " " + window.Title, text);
        }

        private static bool StringContains(string text, string partial)
        {
            return partial.Split(' ').All(word => CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, word, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) > -1);
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
                    ScrollToNextWindowEntry();
                    break;

                case Key.Up:
                    ScrollToPreviousWindowEntry();
                    break;

                default:
                    return;
            }

            e.Handled = true;
        }

        private void ScrollToPreviousWindowEntry()
        {
            if (WindowsListView.SelectedIndex <= 0)
                return;

            WindowsListView.SelectedIndex--;
            WindowsListView.ScrollIntoView(WindowsListView.SelectedItem);
        }

        private void ScrollToNextWindowEntry()
        {
            if (WindowsListView.SelectedIndex >= WindowsListView.Items.Count - 1)
                return;

            WindowsListView.SelectedIndex++;
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
            Activate();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!_isClosing)
                Close();
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
