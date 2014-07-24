using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GoToWindow.Api;

namespace GoToWindow
{
    public partial class MainForm : Form
    {
	    private WindowsList _windowsList;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
			ActiveControl = searchTextBox;

	        _windowsList = WindowsListFactory.Load();
            windowsListBox.Items.AddRange(_windowsList.Windows.ToArray<Object>());
            windowsListBox.SelectedIndex = 0;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void goToWindowButton_Click(object sender, EventArgs e)
        {
            var selectedItem = (IWindow)windowsListBox.SelectedItem;

            if (selectedItem != null)
            {
                try
                {
                    selectedItem.Focus();

                }
                catch(Exception exc)
                {
                    MessageBox.Show("There was an error trying to switch: " + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Application.Exit();
        }

		private void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			var currentSelection = windowsListBox.SelectedItem;

			windowsListBox.BeginUpdate();
			windowsListBox.Items.Clear();
			windowsListBox.Items.AddRange(Filter(_windowsList.Windows, searchTextBox.Text).ToArray<Object>());
			windowsListBox.EndUpdate();

			if (currentSelection != null && windowsListBox.Items.Contains(currentSelection))
				windowsListBox.SelectedItem = currentSelection;
			else if(windowsListBox.Items.Count > 0)
				windowsListBox.SelectedIndex = 0;
		}

		private void searchTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (windowsListBox.Items.Count == 0)
				return;

			if (e.KeyCode == Keys.Down && windowsListBox.SelectedIndex < windowsListBox.Items.Count - 1)
			{
				windowsListBox.SelectedIndex++;
			}

			if (e.KeyCode == Keys.Up && windowsListBox.SelectedIndex > 0)
			{
				windowsListBox.SelectedIndex--;
			}
		}

	    private static IEnumerable<IWindow> Filter(IEnumerable<IWindow> windows, string searchText)
	    {
		    return string.IsNullOrEmpty(searchText)
				? windows
				: windows.Where(window => CultureInfo.CurrentUICulture.CompareInfo.IndexOf(window.Title, searchText, CompareOptions.IgnoreCase) > -1);
	    }
    }
}
