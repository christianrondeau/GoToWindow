using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GoToWindow.Plugins.Core.Utils
{
	public static class IconLoader
	{
		public static BitmapFrame LoadIcon(IntPtr iconHandle, string executableFile)
		{
			return ConvertFromHandle(iconHandle) ?? ConvertFromFile(executableFile);
		}

		private static BitmapFrame ConvertFromHandle(IntPtr iconHandle)
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
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
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
	}
}