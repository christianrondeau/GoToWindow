using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GoToWindow.Api;
using System.Data;
using Equin.ApplicationFramework;
using System.Drawing;

namespace GoToWindow
{
    public partial class MainForm : Form
    {
	    private IList<IWindow> _windows;
		private readonly IGoToWindowApplication _app;
        private BindingListView<IWindow> _windowsBindingListView;

		public MainForm(IGoToWindowApplication app)
        {
	        _app = app;

	        InitializeComponent();

            TopMost = AppConfiguration.Current.AlwaysOnTop;
            ShowInTaskbar = AppConfiguration.Current.AlwaysShow;
        }

		public void InitializeData(IList<IWindow> windows)
	    {
			_windows = windows;

			searchTextBox.Text = "";
			ActiveControl = searchTextBox;

            _windowsBindingListView = new BindingListView<IWindow>(windows.ToList());
            windowsDataGrid.DataSource = _windowsBindingListView;
            windowsDataGrid.Columns["HWnd"].Visible = false;
            windowsDataGrid.Columns["ProcessName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            windowsDataGrid.Columns["ProcessName"].CellTemplate.Style.ForeColor = Color.Gray;
            windowsDataGrid.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            
            SelectFirstWindow();
	    }

        private void SelectFirstWindow()
        {
            if (windowsDataGrid.Rows.Count > 0)
                windowsDataGrid.Rows[0].Selected = true;
        }

        private ListViewItem GetListViewItem(IWindow window)
        {
            var item = new ListViewItem(window.Title);

            item.SubItems.Add(window.Title);

            return item;
        }

        private void GoToSelectedWindow()
        {
            if (windowsDataGrid.SelectedRows.Count == 0)
            {
                _app.Hide();
                return;
            }

            var selectedItem = GetRowWindow(windowsDataGrid.SelectedRows[0]);

            if (selectedItem != null)
            {
                try
                {
                    selectedItem.Focus();
                }
                catch (Exception exc)
                {
                    MessageBox.Show("There was an error trying to switch: " + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _app.Hide();
        }

        private IWindow GetRowWindow(DataGridViewRow row)
        {
            return ((ObjectView<IWindow>)row.DataBoundItem).Object;
        }

        private void UpdateSearchText()
        {
            IWindow selectedWindow = null;

            if (windowsDataGrid.SelectedRows.Count > 0)
                selectedWindow = GetRowWindow(windowsDataGrid.SelectedRows[0]);

            _windowsBindingListView.ApplyFilter(SearchFilter);

            foreach(DataGridViewRow row in windowsDataGrid.Rows)
            {
                if(GetRowWindow(row) == selectedWindow)
                {
                    row.Selected = true;
                    break;
                }
            }
        }

        private bool SearchFilter(IWindow window)
        {
            if (string.IsNullOrEmpty(searchTextBox.Text))
                return true;

            return StringContains(window.Title, searchTextBox.Text) || StringContains(window.ProcessName, searchTextBox.Text);
        }

        private bool StringContains(string text, string partial)
        {
            return CultureInfo.CurrentUICulture.CompareInfo.IndexOf(text, partial, CompareOptions.IgnoreCase) > -1;
        }

        private void MoveInList(KeyEventArgs e)
        {
            if (windowsDataGrid.Rows.Count == 0)
                return;

            if (windowsDataGrid.SelectedRows.Count == 0)
            {
                SelectFirstWindow();
                return;
            }
            
            if (e.KeyCode == Keys.Down && windowsDataGrid.SelectedRows[0].Index < windowsDataGrid.Rows.Count - 1)
            {
                windowsDataGrid.Rows[windowsDataGrid.SelectedRows[0].Index + 1].Selected = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Up && windowsDataGrid.SelectedRows[0].Index > 0)
            {
                windowsDataGrid.Rows[windowsDataGrid.SelectedRows[0].Index - 1].Selected = true;
                e.SuppressKeyPress = true;
                return;
            }
        }

        #region Events

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _app.Hide();
        }

        private void goToWindowButton_Click(object sender, EventArgs e)
        {
            GoToSelectedWindow();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSearchText();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            _app.Hide();
        }

        #endregion

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            MoveInList(e);
        }

    }
}
