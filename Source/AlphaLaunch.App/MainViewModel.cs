using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlphaLaunch.App.Indexes;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;
        private int _selectedIndex;

        public MainViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {

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
            var items = IndexStore.Instance.Search(search);
            
            Items.Clear();

            foreach (var item in items.Select(x => new SearchItemModel(x.Name, x.Id)))
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

        public void OpenSelected()
        {
            if (!Items.Any())
            {
                return;
            }

            var searchItemModel = Items[_selectedIndex];

            var fileItem = IndexStore.Instance.GetById(searchItemModel.Id);

            var info = new ProcessStartInfo(Path.Combine(fileItem.DirectoryName, fileItem.Name));
            Process.Start(info);
        }
    }
}