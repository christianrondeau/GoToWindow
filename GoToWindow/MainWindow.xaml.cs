using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private TrayIcon _trayIcon;

        public MainWindow()
        {

            InitializeComponent();

            Icon = GetIcon();
            ShowInTaskbar = false;
        }

        private static BitmapFrame GetIcon()
        {
            MemoryStream iconStream = new MemoryStream();
            Properties.Resources.AppIcon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            return BitmapFrame.Create(iconStream);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            _trayIcon = new TrayIcon(this);
            _trayIcon.LeftDoubleClick += new EventHandler(TrayIcon_LeftDoubleClick);
            _trayIcon.Show(Properties.Resources.AppIcon, "Go To Window");
        }

        void TrayIcon_LeftDoubleClick(object sender, EventArgs e)
        {
            WindowState = System.Windows.WindowState.Normal;
        }
    }
}
