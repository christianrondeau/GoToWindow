using System;
using System.Globalization;
using System.Windows.Data;

namespace GoToWindow.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public class IncrementIntConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return value;

			return System.Convert.ToInt32(value) + System.Convert.ToInt32(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return value;

			return System.Convert.ToInt32(value) - System.Convert.ToInt32(parameter);
		}
	}
}
