using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoToWindow
{
    public interface IGoToWindowContext
    {
        void Show();
        void Hide();
    }

    public class GoToWindowContext : IGoToWindowContext
    {
        private MainWindow _mainWindow;

        public void Show()
        {
            Hide();

            _mainWindow = new MainWindow();
            _mainWindow.Closing += _mainWindow_Closing;
            _mainWindow.Show();
        }

        void _mainWindow_Closing(object sender, EventArgs e)
        {
            _mainWindow = null;
        }

        public void Hide()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
                _mainWindow.Close();
        }
    }
}
