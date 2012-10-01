using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;
        private FileItem[] _fileItems = new FileItem[0];

        public MainViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
            Items.Add(new SearchItemModel("This is a test."));
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

        private void UpdateSearch(string search)
        {
            var items = _fileItems.Where(x => x.Name.StartsWith(search)).Take(10);

            Items.Clear();

            foreach (var item in items.Select(x => new SearchItemModel(x.Name)))
            {
                Items.Add(item);
            }
        }

        public ObservableCollection<SearchItemModel> Items { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public int IndexDropbox()
        {
            var dropbox = new DirectoryInfo(@"C:\Users\rasmuskl\Dropbox");
            _fileItems = GetFiles(dropbox).ToArray();

            return _fileItems.Length;
        }

        private IEnumerable<FileItem> GetFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Select(x => new FileItem(x.DirectoryName, x.Name, x.Extension))
                .Concat(directory.GetDirectories().SelectMany(GetFiles));
        }

    }
}