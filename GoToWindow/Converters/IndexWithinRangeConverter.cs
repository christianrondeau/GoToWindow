using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GoToWindow.Converters
{
	public class IndexWithinRangeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (parameter == null) throw new ArgumentNullException(nameof(parameter));

			var range = int.Parse(parameter.ToString());
			var item = (ListViewItem)value;

			if (!(ItemsControl.ItemsControlFromItemContainer(item) is ListView listView))
				return false;

			var index = listView.ItemContainerGenerator.IndexFromContainer(item);
			return (index + 1 <= range) ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
