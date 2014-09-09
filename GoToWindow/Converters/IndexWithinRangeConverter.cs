using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GoToWindow.Converters
{
	public class IndexWithinRangeConverter : IValueConverter
	{
		public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
		{
			var range = Int32.Parse(parameter.ToString());
			ListViewItem item = (ListViewItem)value;
			ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
			int index = listView.ItemContainerGenerator.IndexFromContainer(item);
			return (index + 1 <= range) ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
