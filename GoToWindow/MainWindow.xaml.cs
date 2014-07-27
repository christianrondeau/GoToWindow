using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {

            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var windows = WindowsListFactory.Load();
            windowsListView.ItemsSource = windows.Windows;
        }

        private void windowsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var windowEntry = windowsListView.SelectedItem as IWindowEntry;
            if (windowEntry != null)
            {
                windowEntry.Focus();
                Close();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(windowsListView.ItemsSource);
            view.Filter = item => SearchFilter((IWindowEntry)item, searchTextBox.Text);
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
    }
}
