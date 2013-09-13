using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AlphaLaunch.App
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private int _selectedIndex;

        public ListViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
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

        public ObservableCollection<SearchItemModel> Items { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}