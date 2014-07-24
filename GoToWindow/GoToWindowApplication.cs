using System;
using System.Windows.Forms;
using GoToWindow.Api;

namespace GoToWindow
{
	public class GoToWindowApplication : IGoToWindowApplication
	{
		private MainForm _mainForm;
		private NotifyIcon _notifyIcon;

		public void Start(NotifyIcon notifyIcon)
		{
			_mainForm = new MainForm(this);

			_notifyIcon = notifyIcon;
			_notifyIcon.Visible = true;
		}
		
		public void Exit()
		{
			_notifyIcon.Visible = false;
			Application.Exit();
		}
		
		public void Show()
		{
			var windowsList = WindowsListFactory.Load();

			_mainForm.InitializeData(windowsList.Windows);

			if (_mainForm.Visible)
				_mainForm.Activate();
			else
				_mainForm.Show();
		}

		public void Hide()
		{
			if (_mainForm.Visible)
				_mainForm.Hide();
		}
	}
}