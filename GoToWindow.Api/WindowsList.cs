using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GoToWindow.Api
{
    public class Window
    {
        public string Title { get; set; }
    }

    public class WindowsList
    {
        public static WindowsList Load()
        {
            var list = new WindowsList();

            var processes = Process.GetProcesses();

            list.Windows = processes.Select(process => new Window { Title = process.MainWindowTitle }).ToList();

            return list;
        }

        public IList<Window> Windows { get; private set; }
    }
}
