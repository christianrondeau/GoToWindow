using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GoToWindow.Extensibility.Controls;

namespace GoToWindow.Extensibility.ViewModel
{
	public class DesignTimeSearchResult : IBasicSearchResult
	{
		public UserControl View { get; private set; }

		public BitmapFrame Icon { get; private set; }
		public string Title { get; private set; }
		public string ProcessName { get; private set; }
		public string Error { get; set; }

		public DesignTimeSearchResult()
			: this(null, "process", "Window Title")
		{
		}

		public DesignTimeSearchResult(BitmapFrame icon, string processName, string title)
		{
			View = new BasicListEntry();
			Icon = icon;
			ProcessName = processName;
			Title = title;
		}

		public void Select()
		{
		}

		public bool IsShown(string searchQuery)
		{
			return true;
		}
	}
}
