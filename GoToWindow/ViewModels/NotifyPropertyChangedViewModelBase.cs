using System.ComponentModel;

namespace GoToWindow.ViewModels
{
	public abstract class NotifyPropertyChangedViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
