using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Catapult.App.Properties;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.App
{
    public class SearchItemModel : INotifyPropertyChanged
    {
        private BitmapFrame _icon;

        public SearchItemModel(string name, double score, IIndexable targetItem, ImmutableHashSet<int> highlightIndexes, IIconResolver iconResolver)
        {
            Name = name;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
            InitIcon = LoadIconAsync(iconResolver);
        }

        public string Name { get; set; }
        public double Score { get; set; }
        public IIndexable TargetItem { get; set; }
        public Guid Id { get; set; }
        public ImmutableHashSet<int> HighlightIndexes { get; set; }

        public BitmapFrame Icon
        {
            get { return _icon; }
            set
            {
                if (Equals(value, _icon)) return;
                _icon = value;
                OnPropertyChanged();
            }
        }

        public Task InitIcon { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly ConcurrentDictionary<string, BitmapFrame> IconCache = new ConcurrentDictionary<string, BitmapFrame>();

        private async Task LoadIconAsync(IIconResolver iconResolver)
        {
            BitmapFrame frame;
            if (IconCache.TryGetValue(TargetItem.BoostIdentifier, out frame))
            {
                Icon = frame;
                return;
            }

            var bitmapFrame = await Task.Factory.StartNew(() =>
            {
                var icon = iconResolver?.Resolve();

                if (icon == null)
                {
                    return null;
                }

                using (var bmp = icon.ToBitmap())
                {
                    var stream = new MemoryStream();
                    bmp.Save(stream, ImageFormat.Png);
                    return BitmapFrame.Create(stream);
                }
            });

            if (bitmapFrame == null)
            {
                return;
            }

            IconCache.AddOrUpdate(TargetItem.BoostIdentifier, bitmapFrame, (x, f) => bitmapFrame);

            Icon = bitmapFrame;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}