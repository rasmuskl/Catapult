using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;
using Serilog;

namespace Catapult.App
{
    public class IconService
    {
        public static readonly IconService Instance = new IconService();
        private readonly BlockingCollection<IconRequest> _queue = new BlockingCollection<IconRequest>(new ConcurrentQueue<IconRequest>());
        private readonly Dictionary<string, BitmapFrame> _iconCache = new Dictionary<string, BitmapFrame>();


        private IconService()
        {
            new Thread(ProcessQueue) { IsBackground = true }.Start();
        }

        private void ProcessQueue()
        {
            foreach (IconRequest request in _queue.GetConsumingEnumerable())
            {
                try
                {
                    if (request.Model.Disposed)
                    {
                        continue;
                    }

                    IIndexable targetItem = request.Model.TargetItem;
                    var cacheKey = $"{targetItem.GetType()}: {targetItem.Name}";

                    BitmapFrame frame;
                    if (_iconCache.TryGetValue(cacheKey, out frame))
                    {
                        request.Model.Icon = frame;
                        continue;
                    }

                    IIconResolver iconResolver = targetItem.GetIconResolver();

                    var icon = iconResolver?.Resolve();

                    if (icon == null)
                    {
                        continue;
                    }

                    using (Bitmap bmp = icon.ToBitmap())
                    {
                        var stream = new MemoryStream();
                        bmp.Save(stream, ImageFormat.Png);
                        request.Model.Icon = BitmapFrame.Create(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Icon resolve failed for {Name}.", request.Model.Name);
                }
            }
        }

        public void Enqueue(IconRequest request)
        {
            _queue.Add(request);
        }
    }
}