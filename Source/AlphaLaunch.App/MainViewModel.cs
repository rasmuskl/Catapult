using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Indexes;
using AlphaLaunch.Spotify;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ActionRegistry _actionRegistry;
        private readonly ListViewModel _listModel;
        private string _search;

        public MainViewModel()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<OpenAsAdminAction>();

            RegisterStandaloneAction<SpotifyNextTrackAction>();
            RegisterStandaloneAction<SpotifyPlayPauseAction>();
            RegisterStandaloneAction<SpotifyPreviousTrackAction>();
            RegisterStandaloneAction<SpotifyStopAction>();

            _listModel = new ListViewModel();
        }

        private void RegisterStandaloneAction<T>() where T : IStandaloneAction, new()
        {
            _actionRegistry.RegisterAction<T>();
            IndexStore.Instance.IndexAction(new T());
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
            IEnumerable<SearchResult> items = IndexStore.Instance.Search(search).Take(10);

            ListModel.Items.Clear();

            foreach (var item in items.Select(x => new SearchItemModel(x.Name, x.Score, x.TargetItem, x.HighlightIndexes)))
            {
                ListModel.Items.Add(item);
            }

            ListModel.SelectedIndex = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public ListViewModel ListModel
        {
            get { return _listModel; }
        }

        public void OpenSelected()
        {
            if (!_listModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _listModel.Items[_listModel.SelectedIndex];

            var standaloneAction = searchItemModel.TargetItem as IStandaloneAction;
            if (standaloneAction != null)
            {
                standaloneAction.RunAction();
                IndexStore.Instance.AddBoost(Search, searchItemModel.TargetItem.BoostIdentifier);
                return;
            }

            var actionList = _actionRegistry.GetActionFor(searchItemModel.TargetItem.GetType());

            var firstActionType = actionList.First();

            var actionInstance = Activator.CreateInstance(firstActionType);
            var runMethod = firstActionType.GetMethod("RunAction");
            runMethod.Invoke(actionInstance, new[] { searchItemModel.TargetItem });

            IndexStore.Instance.AddBoost(Search, searchItemModel.TargetItem.BoostIdentifier);
        }
    }
}