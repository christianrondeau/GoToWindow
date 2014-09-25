using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace GoToWindow.Extensibility.Controls
{
	public partial class BasicListEntry : UserControl
	{
		public BasicListEntry()
		{
			InitializeComponent();

		    if (!DesignerProperties.GetIsInDesignMode(this))
		        Background = Brushes.Transparent;
		}
	}
}
