using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;
        private int _selectedIndex;
        private readonly ActionRegistry _actionRegistry;

        public MainViewModel()
        {
            Items = new ObservableCollection<SearchItemModel>();
            PropertyChanged += OnPropertyChanged;

            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<OpenAsAdminAction>();
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
            IEnumerable<SearchResult> items = IndexStore.Instance.Search(search);

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

        public void OpenSelected()
        {
            if (!Items.Any())
            {
                return;
            }

            var searchItemModel = Items[_selectedIndex];

            var actionList = _actionRegistry.GetActionFor(searchItemModel.TargetItem.GetType());

            var firstActionType = actionList.First();

            var actionInstance = Activator.CreateInstance(firstActionType);
            var runMethod = firstActionType.GetMethod("RunAction");
            runMethod.Invoke(actionInstance, new[] { searchItemModel.TargetItem });
        }
    }
}