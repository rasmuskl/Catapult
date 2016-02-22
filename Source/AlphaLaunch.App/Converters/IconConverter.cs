using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AlphaLaunch.Core.Indexes;
using Serilog;

namespace AlphaLaunch.App.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                var iconResolver = value as IIconResolver;

                var icon = iconResolver?.Resolve();

                if (icon == null)
                {
                    return null;
                }

                using (Bitmap bmp = icon.ToBitmap())
                {
                    var stream = new MemoryStream();
                    bmp.Save(stream, ImageFormat.Png);
                    return BitmapFrame.Create(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to resolve icon");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}