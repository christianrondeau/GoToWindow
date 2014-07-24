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
	    private IList<IWindow> _windows;
		private readonly IGoToWindowApplication _app;

		public MainForm(IGoToWindowApplication app)
        {
	        _app = app;

	        InitializeComponent();
        }

		public void InitializeData(IList<IWindow> windows)
	    {
			_windows = windows;

			searchTextBox.Text = "";
			ActiveControl = searchTextBox;

			windowsListBox.BeginUpdate();
			windowsListBox.Items.Clear();
			windowsListBox.Items.AddRange(_windows.ToArray<Object>());
			windowsListBox.SelectedIndex = 0;
			windowsListBox.EndUpdate();
	    }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _app.Hide();
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

	        _app.Hide();
        }

		private void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			var currentSelection = windowsListBox.SelectedItem;

			windowsListBox.BeginUpdate();
			windowsListBox.Items.Clear();
			windowsListBox.Items.AddRange(Filter(_windows, searchTextBox.Text).ToArray<Object>());
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
