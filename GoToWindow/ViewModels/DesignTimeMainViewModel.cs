using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GoToWindow.Extensibility;
using GoToWindow.Extensibility.Controls;
using GoToWindow.Extensibility.ViewModel;

namespace GoToWindow.ViewModels
{
	public class DesignTimeMainViewModel : MainViewModel
	{
		private class DesignTimeSearchResult : ISearchResult, IBasicSearchResult
		{
			public UserControl View { get; private set; }

			public BitmapFrame Icon { get; private set; }
			public string Title { get; private set; }
			public string ProcessName { get; private set; }

			public DesignTimeSearchResult(string processName, string title)
			{
				View = new BasicListEntry();
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

		public DesignTimeMainViewModel()
		{
			IsEmpty = false;
			AvailableWindowWidth = 640;
			AvailableWindowHeight = 320;
			//SearchText = "User Query...";
			Windows = new CollectionViewSource
			{
				Source = new List<ISearchResult>
				{
					new DesignTimeSearchResult("process", "Window Title"),
					new DesignTimeSearchResult("very long process name", "Very very long window title that should end up with ellipsis because it is so very long"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title"),
					new DesignTimeSearchResult("filler", "Some Window Title")
				}
			};
		}
	}
}