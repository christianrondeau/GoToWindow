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
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void goToWindowButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
