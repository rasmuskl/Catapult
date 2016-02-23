using System;
using System.Collections.Generic;
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
        static readonly Dictionary<string, BitmapFrame> IconCache = new Dictionary<string, BitmapFrame>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                var iconResolver = value as IIconResolver;

                var iconKey = iconResolver?.IconKey;

                BitmapFrame frame;
                if (iconKey != null && IconCache.TryGetValue(iconKey, out frame))
                {
                    return frame;
                }

                var icon = iconResolver?.Resolve();

                if (icon == null)
                {
                    return null;
                }

                using (Bitmap bmp = icon.ToBitmap())
                {
                    var stream = new MemoryStream();
                    bmp.Save(stream, ImageFormat.Png);
                    var bitmapFrame = BitmapFrame.Create(stream);

                    if (iconKey != null)
                    {
                        IconCache[iconKey] = bitmapFrame;
                    }

                    return bitmapFrame;
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