using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlphaLaunch.App.Debug;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;
        private readonly List<FileItem> _fileItems = new List<FileItem>();

        public MainViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
            Items.Add(new SearchItemModel("This is a test."));
            PropertyChanged += OnPropertyChanged;


            IndexDirectory("Start menu", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            IndexDirectory("Common start menu", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            IndexDirectory("Dropbox", @"C:\Users\rasmuskl\Dropbox");
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
            var items = _fileItems
                .Where(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1)
                .OrderBy(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase))
                .Take(10);

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

        public void IndexDirectory(string s, string path)
        {
            var stopwatch = Stopwatch.StartNew();

            var dropbox = new DirectoryInfo(path);
            var fileItems = GetFiles(dropbox).ToArray();
            _fileItems.AddRange(fileItems);

            stopwatch.Stop();

            Log.Info("Indexed " + s + " - " + fileItems.Length + " items. [" + stopwatch.ElapsedMilliseconds + " ms]");
        }

        private IEnumerable<FileItem> GetFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Select(x => new FileItem(x.DirectoryName, x.Name, x.Extension))
                .Concat(directory.GetDirectories().SelectMany(GetFiles));
        }

    }
}