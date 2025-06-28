using System.Collections.Specialized;
using System.ComponentModel;
using Catapult.Core.Indexes;

namespace Catapult.App;

public class ListViewModel : INotifyPropertyChanged
{
    private int _selectedIndex;
    private IIndexable _selectedIndexable;

    public ListViewModel()
    {
        Items = new SmartObservableCollection<SearchItemModel>();
        Items.CollectionChanged += Items_CollectionChanged;
    }

    private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CheckSelectedIndexableChanged();
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnPropertyChanged("SelectedIndex");
                CheckSelectedIndexableChanged();
            }
        }
    }

    public SearchItemModel SelectedSearchItem
    {
        get
        {
            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                return Items[SelectedIndex];
            }

            return null;
        }
    }

    private void CheckSelectedIndexableChanged()
    {
        if (_selectedIndexable == SelectedSearchItem?.TargetItem)
        {
            return;
        }

        _selectedIndexable = SelectedSearchItem?.TargetItem;
        SelectedSearchItemChanged?.Invoke(_selectedIndexable);
    }

    public Action<IIndexable> SelectedSearchItemChanged = x => { };

    public SmartObservableCollection<SearchItemModel> Items { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}