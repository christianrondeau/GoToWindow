using System;
using System.Windows;

namespace GoToWindow.FakeApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
                throw new ApplicationException("Fake App requires a title as the command line argument");

            Title = args[1];
        }
    }
}
