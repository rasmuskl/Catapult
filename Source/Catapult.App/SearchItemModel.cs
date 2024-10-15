using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Catapult.Core.Indexes;
using JetBrains.Annotations;

namespace Catapult.App;

public sealed class SearchItemModel : INotifyPropertyChanged, IDisposable
{
    private BitmapSource? _icon;

    private SearchItemModel(string name, string details, double score, IIndexable targetItem, ImmutableHashSet<int> highlightIndexes)
    {
        Name = name;
        Details = details;
        Score = score;
        TargetItem = targetItem;
        HighlightIndexes = highlightIndexes;
    }

    public SearchItemModel(SearchResult result) : this(result.Name, result.TargetItem.Details, result.Score, result.TargetItem, result.HighlightIndexes)
    {
    }

    public string Name { get; set; }
    public string Details { get; set; }

    public double Score { get; set; }

    public IIndexable TargetItem { get; set; }

    public Guid Id { get; set; }

    public ImmutableHashSet<int> HighlightIndexes { get; set; }

    public BitmapSource? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        Disposed = true;
    }

    public bool Disposed { get; private set; }
}

public class IconRequest
{
    public SearchItemModel Model { get; }

    public IconRequest(SearchItemModel model)
    {
        Model = model;
    }
}