using System.ComponentModel;
using System.Runtime.CompilerServices;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.App
{
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
                SelectedItemDetails = _selectedItem?.GetDetails();
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
    }
}