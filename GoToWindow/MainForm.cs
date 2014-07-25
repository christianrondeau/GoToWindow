using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GoToWindow.Api;
using Equin.ApplicationFramework;
using System.Drawing;

namespace GoToWindow
{
    public partial class MainForm : Form
    {
        private readonly IGoToWindowApplication _app;
        private readonly BindingListView<IWindow> _windowsBindingListView;

		public MainForm(IGoToWindowApplication app)
        {
	        _app = app;

	        InitializeComponent();

            TopMost = AppConfiguration.Current.AlwaysOnTop;
            ShowInTaskbar = AppConfiguration.Current.AlwaysShow;

            _windowsBindingListView = new BindingListView<IWindow>(new IWindow[0]);

		    windowsDataGrid.VirtualMode = true;
            windowsDataGrid.DataSource = _windowsBindingListView;

            windowsDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            windowsDataGrid.RowTemplate.Height = 32;
            
            // ReSharper disable PossibleNullReferenceException
            windowsDataGrid.Columns["HWnd"].Visible = false;
            windowsDataGrid.Columns["ProcessName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            windowsDataGrid.Columns["ProcessName"].CellTemplate.Style.BackColor = windowsDataGrid.BackColor;
            windowsDataGrid.Columns["ProcessName"].CellTemplate.Style.ForeColor = Color.Gray;
            windowsDataGrid.Columns["Executable"].Visible = false;
            windowsDataGrid.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            // ReSharper restore PossibleNullReferenceException

            var iconsColumn = new DataGridViewImageColumn(true)
            {
                Name = "Icon",
                HeaderText = "Icon",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 32,
                ImageLayout = DataGridViewImageCellLayout.Stretch,
                DefaultCellStyle = { NullValue = null }
            };
            windowsDataGrid.Columns.Insert(0, iconsColumn);
        }

		public void InitializeData(IList<IWindow> windows)
	    {
		    ActiveControl = searchTextBox;

            
		    _windowsBindingListView.DataSource = windows.ToList();

            
            SelectFirstWindow();
	    }

        private void SelectFirstWindow()
        {
            if (windowsDataGrid.Rows.Count > 0)
                windowsDataGrid.Rows[0].Selected = true;
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
                    MessageBox.Show(string.Format("There was an error trying to switch: {0}", exc.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _app.Hide();
        }

        private static IWindow GetRowWindow(DataGridViewRow row)
        {
            if (row.Cells.Count == 0)
                return null;

            return ((ObjectView<IWindow>)row.DataBoundItem).Object;
        }

        private void UpdateSearchText()
        {
            IWindow selectedWindow = null;

            if (windowsDataGrid.SelectedRows.Count > 0)
                selectedWindow = GetRowWindow(windowsDataGrid.SelectedRows[0]);

            _windowsBindingListView.ApplyFilter(SearchFilter);

            foreach (var row in windowsDataGrid.Rows.Cast<DataGridViewRow>().Where(row => GetRowWindow(row) == selectedWindow))
            {
                row.Selected = true;
                break;
            }
        }

        private bool SearchFilter(IWindow window)
        {
            if (string.IsNullOrEmpty(searchTextBox.Text))
                return true;

            return StringContains(window.Title, searchTextBox.Text) || StringContains(window.ProcessName, searchTextBox.Text);
        }

        private static bool StringContains(string text, string partial)
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
            }
            else if (e.KeyCode == Keys.Down && windowsDataGrid.SelectedRows[0].Index < windowsDataGrid.Rows.Count - 1)
            {
                windowsDataGrid.Rows[windowsDataGrid.SelectedRows[0].Index + 1].Selected = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up && windowsDataGrid.SelectedRows[0].Index > 0)
            {
                windowsDataGrid.Rows[windowsDataGrid.SelectedRows[0].Index - 1].Selected = true;
                e.SuppressKeyPress = true;
            }

            windowsDataGrid.CurrentCell = windowsDataGrid.SelectedRows[0].Cells[0];
        }

        #region Events

// ReSharper disable InconsistentNaming

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

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            MoveInList(e);
        }

        private void windowsDataGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            BindRow(e);
        }

        private void windowsDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GoToSelectedWindow();
        }

        private static readonly Keys[] _allowedKeys = {Keys.Down, Keys.Up, Keys.PageDown, Keys.PageUp};

        private void windowsDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GoToSelectedWindow();
                return;
            }

            if (!_allowedKeys.Contains(e.KeyCode))
                e.SuppressKeyPress = true;
        }

        // ReSharper restore InconsistentNaming

        #endregion

        public void Clear()
        {
            searchTextBox.Text = "";
        }

        private void BindRow(DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= 0 && windowsDataGrid.Columns[e.ColumnIndex].Name != "Icon")
                return;

            var row = windowsDataGrid.Rows[e.RowIndex];
            var window = GetRowWindow(row);
            e.Value = Icon.ExtractAssociatedIcon(window.Executable);
        }
    }
}
