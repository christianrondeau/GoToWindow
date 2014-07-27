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
    }

    public class GoToWindowContext : IGoToWindowContext
    {
        private MainWindow _mainWindow;

        public void Show()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
                _mainWindow.Close();

            _mainWindow = new MainWindow();
            _mainWindow.Show();
        }
    }
}
