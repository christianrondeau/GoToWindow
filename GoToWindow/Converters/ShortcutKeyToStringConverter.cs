using System;
using System.Globalization;
using System.Windows.Data;
using GoToWindow.Api;

namespace GoToWindow.Converters
{
	public class ShortcutKeyToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Int32))
				throw new NotSupportedException("Only Int32 can be converted to a key");

			var description = VirtualKeyDescription.GetVirtualKeyDescription((int) value);
			return description ?? "?";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
