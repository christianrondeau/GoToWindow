using System.ComponentModel;
using System.Windows.Media;

namespace GoToWindow.Extensibility.Controls
{
	public partial class BasicListEntry
	{
		public BasicListEntry()
		{
			InitializeComponent();

		    if (!DesignerProperties.GetIsInDesignMode(this))
		        Background = Brushes.Transparent;
		}
	}
}
