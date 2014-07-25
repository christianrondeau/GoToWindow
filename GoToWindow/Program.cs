using System;
using System.Threading;
using System.Windows.Forms;

namespace GoToWindow
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (new Mutex(true, "GoToWindow", out createdNew))
            {
                if (!createdNew) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GoToWindowApplicationContext());
            }
        }
    }
}
