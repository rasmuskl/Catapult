using System.ComponentModel;
using System.Runtime.CompilerServices;
using Catapult.Core.Indexes;

namespace Catapult.App;

public class DetailsViewModel : INotifyPropertyChanged
{
    private IIndexable _selectedItem;
    private object _selectedItemDetails;
    public event PropertyChangedEventHandler PropertyChanged;


    public IIndexable SelectedItem
    {
        get { return _selectedItem; }
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public object SelectedItemDetails
    {
        get { return _selectedItemDetails; }
        set
        {
            _selectedItemDetails = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task SetSelectedAsync(IIndexable indexable)
    {
        if (indexable == null)
        {
            SelectedItem = null;
            SelectedItemDetails = null;
            return;
        }

        var details = await Task.Factory.StartNew(indexable.GetDetails);

        SelectedItem = indexable;
        SelectedItemDetails = details;
    }
}