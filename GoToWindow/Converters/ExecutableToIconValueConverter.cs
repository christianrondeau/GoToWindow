using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GoToWindow.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapFrame))]
    public class ExecutableToIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var iconStream = new MemoryStream();

            using(var icon = Icon.ExtractAssociatedIcon((string)value))
            {
                if (icon == null)
                    return null;

                using (var bmp = icon.ToBitmap())
                {
                    bmp.Save(iconStream, ImageFormat.Png);
                }
            }

            iconStream.Position = 0;
            var decoder = new PngBitmapDecoder(iconStream, BitmapCreateOptions.None, BitmapCacheOption.None);
            return decoder.Frames.Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
