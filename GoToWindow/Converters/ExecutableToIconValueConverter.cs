using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GoToWindow.Api;

namespace GoToWindow.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapFrame))]
    public class ExecutableToIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var window = value as IWindowEntry;

            if (window == null)
                return null;

            return ConvertFromHandle(window.IconHandle) ?? ConvertFromFile(window.Executable);
        }

        private static object ConvertFromHandle(IntPtr iconHandle)
        {
            if (iconHandle == IntPtr.Zero)
                return null;

            using (var icon = Icon.FromHandle(iconHandle))
            {
                return ConvertFromIcon(icon);
            }
        }

        private static BitmapFrame ConvertFromFile(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            using (var icon = Icon.ExtractAssociatedIcon(path))
            {
                return ConvertFromIcon(icon);
            }
        }

        private static BitmapFrame ConvertFromIcon(Icon icon)
        {
            var iconStream = new MemoryStream();

            using (var bmp = icon.ToBitmap())
            {
                bmp.Save(iconStream, ImageFormat.Png);
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
