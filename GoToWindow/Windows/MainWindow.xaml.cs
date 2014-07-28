using System.Windows.Controls.Primitives;
using GoToWindow.Api;
using GoToWindow.Commands;
using GoToWindow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoToWindow
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _viewModel = MainWindowViewModel.Load();
            _viewModel.Close += viewModel_Close;
            DataContext = _viewModel;
        }

        void viewModel_Close(object sender, EventArgs e)
        {
            Close();
        }

        private void windowsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FocusSelectedWindowItem();
        }

        private void FocusSelectedWindowItem()
        {
            var windowEntry = windowsListView.SelectedItem as IWindowEntry;
            if (windowEntry != null)
            {
                if (!windowEntry.Focus())
                    MessageBox.Show("Could not show window. Try running with elevated privileges (Run as Administrator)", "Could Not Show Window", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                
                Close();
            }
            else if(!String.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                KeyboardSend.KeyDown(KeyboardSend.LWin);
                KeyboardSend.PressKey((byte)'S');
                KeyboardSend.KeyUp(KeyboardSend.LWin);

                Thread.Sleep(100);

                foreach(var c in searchTextBox.Text.Trim())
                {
                    var uc = Char.ToUpper(c);
                    // Spaces, numbers and letters
                    if (uc == 0x20 || uc >= 0x30 && uc <= 0x39 || uc >= 0x41 && uc <= 0x5a)
                    {
                        KeyboardSend.PressKey((byte)uc);
                    }
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentSelection = windowsListView.SelectedItem;

            _viewModel.Windows.View.Filter = item => SearchFilter((IWindowEntry)item, searchTextBox.Text);

            if (windowsListView.SelectedIndex == -1 && windowsListView.Items.Count > 0)
                windowsListView.SelectedIndex = 0;
        }

        private bool SearchFilter(IWindowEntry window, string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return StringContains(window.ProcessName + " " + window.Title, text);
        }

        private static bool StringContains(string text, string partial)
        {
            return partial.Split(' ').All(word => CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, word, CompareOptions.IgnoreCase) > -1);
        }

        private void searchBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            searchTextBox.Focus();
            searchTextBox.CaretIndex = searchTextBox.Text.Length;
        }

        private void searchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                FocusSelectedWindowItem();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down && windowsListView.SelectedIndex < windowsListView.Items.Count - 1)
            {
                windowsListView.SelectedIndex++;
                windowsListView.ScrollIntoView(windowsListView.SelectedItem);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up && windowsListView.SelectedIndex > 0)
            {
                windowsListView.SelectedIndex--;
                windowsListView.ScrollIntoView(windowsListView.SelectedItem);
                e.Handled = true;
                return;
            }
        }

        private void windowsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusSelectedWindowItem();
                e.Handled = true;
                return;
            }
        }

        private void clearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GoToWindowShortcut(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine("Yeah!" + e.Parameter);
        }

        private void windowsListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EventHandler eventHandler = null;
            eventHandler = delegate
            {
                if (windowsListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
                windowsListView.ItemContainerGenerator.StatusChanged -= eventHandler;

                if (windowsListView.Items.Count > 1)
                    windowsListView.SelectedIndex = 1;
                else if (windowsListView.Items.Count > 0)
                    windowsListView.SelectedIndex = 0;
            };
            windowsListView.ItemContainerGenerator.StatusChanged += eventHandler;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Activate();
        }
    }
}
