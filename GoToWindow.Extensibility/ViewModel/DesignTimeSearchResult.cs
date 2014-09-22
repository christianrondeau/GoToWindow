using System;
using System.Globalization;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
			: this(CreateSampleIcon(), "process", "Window Title", "Error Message")
		{
		}

	    public DesignTimeSearchResult(BitmapFrame icon, string processName, string title, string error = null)
		{
			View = new BasicListEntry();
			Icon = icon;
			ProcessName = processName;
			Title = title;
		    Error = error;
		}

		public void Select()
		{
		}

		public bool IsShown(string searchQuery)
		{
			return true;
		}

        public static BitmapFrame CreateSampleIcon()
        {
            var renderTargetBitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Default);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawEllipse(new LinearGradientBrush(Colors.SkyBlue, Colors.SaddleBrown, new Point(0.4, 0), new Point(0.6, 1)), new Pen(Brushes.Black, 1), new Point(16, 16), 15, 15);
                var formattedText = new FormattedText("icon", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Sergoe UI"), 12, Brushes.White)
                {
                    TextAlignment = TextAlignment.Center
                };
                context.DrawText(formattedText, new Point(16, 9));
            }
            renderTargetBitmap.Render(visual);
            return BitmapFrame.Create(renderTargetBitmap);
        }
	}
}
