using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GoToWindow.Converters
{
	public class IndexConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			var item = (ListViewItem)value;
			var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

            if (listView == null)
                return false;

			var index = listView.ItemContainerGenerator.IndexFromContainer(item);
			return (index + 1).ToString(CultureInfo.InvariantCulture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
