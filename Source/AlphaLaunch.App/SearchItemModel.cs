using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AlphaLaunch.App.Annotations;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.App
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

        private async Task LoadIconAsync(IIconResolver iconResolver)
        {
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
                    var frame = BitmapFrame.Create(stream);

                    return frame;
                }
            });

            if (bitmapFrame == null)
            {
                return;
            }

            Icon = bitmapFrame;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}