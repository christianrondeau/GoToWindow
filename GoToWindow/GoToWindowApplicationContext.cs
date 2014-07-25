using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Core;

namespace GoToWindow
{
	/// <remarks>
	/// Source: http://www.codeproject.com/Articles/18683/Creating-a-Tasktray-Application
	/// </remarks>
	public class GoToWindowApplicationContext : ApplicationContext
	{
        private readonly ILog _log = LogManager.GetLogger(typeof(GoToWindowApplicationContext).Assembly, "GoToWindow");
		private readonly GoToWindowApplication _app;
		private InterceptAltTab _altTab;

		public GoToWindowApplicationContext()
		{
			_app = new GoToWindowApplication();

		    AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

			InitializeComponent();
		}

	    private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
	    {
	        _log.Error(e.ExceptionObject);

	        string message;
	        var exc = e.ExceptionObject as Exception;
	        if (exc != null)
	            message = exc.Message;
	        else
	            message = "An unknown error occured: " + e;

	        MessageBox.Show(message, "GoToWindow Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	    }

	    private void InitializeComponent()
		{
			var configMenuItem = new MenuItem("Show", (obj, args) => _app.Show());
			var exitMenuItem = new MenuItem("Exit", (obj, args) => _app.Exit());

			var notifyIcon = new NotifyIcon
			{
				Icon = LoadIconFromEmbeddedResource(),
				ContextMenu = new ContextMenu(new[] {configMenuItem, exitMenuItem}),
                Text = "Go To Window"
			};

			notifyIcon.DoubleClick += (obj, args) => _app.Show();

            _app.Start(notifyIcon);

            if (AppConfiguration.Current.HookAltTab)
                _altTab = new InterceptAltTab(AltTabCallback);
		}

		private void AltTabCallback()
        {
			_app.Show();
		}

		private static Icon LoadIconFromEmbeddedResource()
		{
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			// Icon from http://designmodo.com
			using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".AppIcon.ico"))
			{
			    return stream == null ? null : new Icon(stream);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _altTab != null)
				_altTab.Dispose();

			base.Dispose(disposing);
		}
	}
}
