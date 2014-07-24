using System;
using System.Linq;
using System.Windows.Forms;
using GoToWindow.Api;

namespace GoToWindow
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            windowsListBox.Items.AddRange(WindowsListFactory.Load().Windows.ToArray<Object>());
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
    }
}
