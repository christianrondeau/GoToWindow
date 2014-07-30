using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        public void TabAgain()
        {
            FocusSelectedWindowItem();
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
                SearchInWindowsSearch();
            }
        }

        private void SearchInWindowsSearch()
        {
            KeyboardSend.KeyDown(KeyboardSend.LWin);
            KeyboardSend.KeyPress((byte) 'S');
            KeyboardSend.KeyUp(KeyboardSend.LWin);

            Thread.Sleep(100);

            foreach (var c in SearchTextBox.Text.Trim())
            {
                var uc = Char.ToUpper(c);
                // Spaces, numbers and letters
                if (uc == 0x20 || uc >= 0x30 && uc <= 0x39 || uc >= 0x41 && uc <= 0x5a)
                {
                    KeyboardSend.KeyPress((byte) uc);
                }
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

            if(e.Key == Key.Enter)
            {
                FocusSelectedWindowItem();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down && WindowsListView.SelectedIndex < WindowsListView.Items.Count - 1)
            {
                WindowsListView.SelectedIndex++;
                WindowsListView.ScrollIntoView(WindowsListView.SelectedItem);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up && WindowsListView.SelectedIndex > 0)
            {
                WindowsListView.SelectedIndex--;
                WindowsListView.ScrollIntoView(WindowsListView.SelectedItem);
                e.Handled = true;
                return;
            }
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

                if (WindowsListView.Items.Count > 1)
                    WindowsListView.SelectedIndex = 1;
                else if (WindowsListView.Items.Count > 0)
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
    }
}
