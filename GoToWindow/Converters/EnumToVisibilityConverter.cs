using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GoToWindow.Converters
{
	public class EnumToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (parameter == null) throw new ArgumentNullException(nameof(parameter));

			var desiredValue = (Enum)Enum.Parse(value.GetType(), parameter.ToString());
			return desiredValue.Equals(value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
