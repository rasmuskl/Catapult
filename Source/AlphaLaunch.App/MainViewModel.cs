using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Indexes;
using AlphaLaunch.Core.Selecta;
using AlphaLaunch.Spotify;
using Serilog;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ActionRegistry _actionRegistry;

        private readonly ListViewModel _mainListModel = new ListViewModel();
        private Searcher _selectaSeacher;
        private readonly List<IIndexable> _actions = new List<IIndexable>();

        public MainViewModel()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<OpenAsAdminAction>();

            RegisterAction<SpotifyNextTrackAction>();
            RegisterAction<SpotifyPlayPauseAction>();
            RegisterAction<SpotifyPreviousTrackAction>();
            RegisterAction<SpotifyStopAction>();

            RegisterAction<KillProcessAction>();
            RegisterAction<OpenLastLogAction>();
            RegisterAction<OpenLogFolderAction>();
        }

        private void RegisterAction<T>() where T : IIndexable, new()
        {
            _actionRegistry.RegisterAction<T>();
            IndexStore.Instance.IndexAction(new T());

            _actions.Add(new T());
        }

        public async Task UpdateSearchAsync(string search, CancellationToken token)
        {
            //var searchItemModel = _mainListModel.SelectedSearchItem;

            //if (Search.Contains(" ") && searchItemModel != null && searchItemModel.TargetItem is KillProcessAction)
            //{
            //    MainListModel = _processListModel;

            //    var processes = Process
            //        .GetProcesses()
            //        .Select(x => new RunningProcessInfo(x.ProcessName, x.MainWindowTitle, x.Id));

            //    var searcher = new FuzzySearcher();

            //    searcher.IndexItems(processes);

            //    var searchResults = searcher.Search(Search.Split(new[] { ' ' })[1], ImmutableDictionary.Create<string, EntryBoost>()).Take(10);

            //    _processListModel.Items.Clear();

            //    foreach (var searchResult in searchResults)
            //    {
            //        _processListModel.Items.Add(new SearchItemModel(searchResult.Name, searchResult.Score, searchResult.TargetItem, searchResult.HighlightIndexes));
            //    }

            //    _processListModel.SelectedIndex = 0;

            //    return;
            //}

            var items = await Task.Factory.StartNew(() =>
            {
                _selectaSeacher = _selectaSeacher ?? Searcher.Create(SearchResources.GetFiles().Concat(_actions).ToArray());
                _selectaSeacher = _selectaSeacher.Search(search);
                return _selectaSeacher.SearchResults.Take(10);
            }, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            MainListModel.Items.Clear();

            foreach (var item in items.Select(x => new SearchItemModel(x.Name, x.Score, x.TargetItem, x.HighlightIndexes, x.TargetItem.GetIconResolver())))
            {
                MainListModel.Items.Add(item);
            }

            MainListModel.SelectedIndex = 0;
        }

        public ListViewModel MainListModel
        {
            get { return _mainListModel; }
        }

        public void OpenSelected()
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            //if (_mainListModel.SelectedSearchItem != null)
            //{
            //    var killProcessAction = _mainListModel.SelectedSearchItem.TargetItem as KillProcessAction;

            //    if (killProcessAction != null)
            //    {
            //        var processItem = _processListModel.SelectedSearchItem.TargetItem as RunningProcessInfo;

            //        if (processItem == null)
            //        {
            //            return;
            //        }

            //        killProcessAction.RunAction(processItem);
            //        return;
            //    }
            //}


            var searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];

            var standaloneAction = searchItemModel.TargetItem as IStandaloneAction;
            if (standaloneAction != null)
            {
                Log.Information("Launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, standaloneAction.GetType());

                try
                {
                    standaloneAction.RunAction();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, standaloneAction.GetType());
                }

                return;
            }


            var actionList = _actionRegistry.GetActionFor(searchItemModel.TargetItem.GetType());
            var firstActionType = actionList.First();

            try
            {
                var actionInstance = Activator.CreateInstance(firstActionType);
                var runMethod = firstActionType.GetMethod("RunAction");

                Log.Information("Launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, firstActionType);
                runMethod.Invoke(actionInstance, new[] { searchItemModel.TargetItem });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, firstActionType);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}