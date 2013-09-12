using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.App
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private string _search;
        private int _selectedIndex;

        public ListViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
        }

        public string Search
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    _search = value;
                    OnPropertyChanged("Search");

                    UpdateSearch(_search);
                }
            }
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

        private void UpdateSearch(string search)
        {
            IEnumerable<SearchResult> items = IndexStore.Instance.Search(search).Take(10);

            Items.Clear();

            foreach (var item in items.Select(x => new SearchItemModel(x.Name, x.Score, x.TargetItem, x.HighlightIndexes)))
            {
                Items.Add(item);
            }

            SelectedIndex = 0;
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