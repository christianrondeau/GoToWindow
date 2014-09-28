using GoToWindow.Squirrel;
using System;

namespace GoToWindow.ViewModels
{
	public class UpdateViewModel : NotifyPropertyChangedViewModelBase
	{
		private readonly SquirrelUpdater _updater;

		private UpdateStatus _updateStatus;
		public UpdateStatus UpdateStatus
		{
			get { return _updateStatus; }
			set
			{
				_updateStatus = value;
				OnPropertyChanged("UpdateStatus");
			}
		}
	
		private bool _enabled;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;
				OnPropertyChanged("Enabled");
			}
		}

		private int _progress;
		public int Progress
		{
			get { return _progress; }
			set
			{
				_progress = value;
				OnPropertyChanged("Progress");
			}
		}

		public UpdateViewModel()
		{
			_updater = SquirrelContext.AcquireUpdater();
			Enabled = false;
		}

		public void Update()
		{
			_updater.Update(UpdateCallback, UpdateErrorCallback);
		}

		private void UpdateCallback(UpdateStatus updateStatus, int progress)
		{
			UpdateStatus = updateStatus;
		}

		private void UpdateErrorCallback(Exception exc)
        {
			UpdateStatus = UpdateStatus.Error;
			Enabled = true;
		}
	}
}
