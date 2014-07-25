using System.Windows.Forms;
using GoToWindow.Api;

namespace GoToWindow
{
	public class GoToWindowApplication : IGoToWindowApplication
	{
        public delegate void ShowAsyncDelegate();

		private MainForm _mainForm;
		private NotifyIcon _notifyIcon;

        public void Start(NotifyIcon notifyIcon)
		{
            _mainForm = new MainForm(this);

			_notifyIcon = notifyIcon;
            _notifyIcon.Visible = true;

            if (AppConfiguration.Current.AlwaysShow)
                Show();
		}
		
		public void Exit()
		{
			_notifyIcon.Visible = false;
			Application.Exit();
		}
		
		public void Show()
		{
            if(_mainForm.Visible)
                return;

            _mainForm.Show();
            _mainForm.Activate();

            _mainForm.BeginInvoke(new ShowAsyncDelegate(ShowAsync));
		}

	    private void ShowAsync()
	    {
            var windowsList = WindowsListFactory.Load();
            _mainForm.InitializeData(windowsList.Windows);
	    }

		public void Hide()
		{
            if (AppConfiguration.Current.AlwaysShow)
                return;

			if (_mainForm.Visible)
				_mainForm.Hide();

            _mainForm.Clear();
		}
    }
}