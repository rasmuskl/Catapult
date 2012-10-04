using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlphaLaunch.App.Config;
using AlphaLaunch.App.Debug;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;
        private readonly List<FileItem> _fileItems = new List<FileItem>();
        private int _selectedIndex;

        public MainViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
            PropertyChanged += OnPropertyChanged;

            var loader = new JsonConfigLoader();
            var config = loader.Load("config.json");
            loader.Save(config, "config.json");
            
            IndexDirectory("Start menu", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            IndexDirectory("Common start menu", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

            foreach (var path in config.Paths)
            {
                IndexDirectory(path, path);

                //IndexDirectory("Dropbox", @"C:\Users\rasmuskl\Dropbox");
            }

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
            var items = _fileItems
                .Where(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1)
                .OrderBy(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase))
                .Take(10);

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

        public void OpenSelected()
        {
            if (!Items.Any())
            {
                return;
            }

            var searchItemModel = Items[_selectedIndex];

            var fileItem = _fileItems.FirstOrDefault(x => x.Id == searchItemModel.Id);

            var info = new ProcessStartInfo(Path.Combine(fileItem.DirectoryName, fileItem.Name));
            Process.Start(info);
        }
    }
}