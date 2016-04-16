using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Frecency;
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
        private readonly List<IIndexable> _actions = new List<IIndexable>();
        private readonly FrecencyStorage _frecencyStorage;
        private readonly Stack<ISearchFrame> _stack = new Stack<ISearchFrame>();
        private readonly Stack<IIndexable> _selectedIndexables = new Stack<IIndexable>();

        public event Action StackPushed;

        public MainViewModel()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();

            var frecencyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AlphaLaunch", "frecency.json");
            _frecencyStorage = new FrecencyStorage(frecencyPath);

            RegisterAction<SpotifyNextTrackAction>();
            RegisterAction<SpotifyPlayPauseAction>();
            RegisterAction<SpotifyPreviousTrackAction>();
            RegisterAction<SpotifyStopAction>();

            RegisterAction<KillProcessAction>();
            RegisterAction<OpenLastLogAction>();
            RegisterAction<OpenLogFolderAction>();

            RegisterAction<GoogleAction>();
            RegisterAction<PathOfExileWikiAction>();
            RegisterAction<WikipediaAction>();

            _stack.Push(new IndexableSearchFrame(SearchResources.GetFiles().Concat(_actions).ToArray()));

            StartIntentService(Dispatcher.CurrentDispatcher);
        }

        private void RegisterAction<T>() where T : IIndexable, new()
        {
            _actionRegistry.RegisterAction<T>();
            _actions.Add(new T());
        }

        private void UpdateSearchItems(SearchItemModel[] searchItemModels)
        {
            MainListModel.Items.Reset(searchItemModels);
            MainListModel.SelectedIndex = 0;
        }

        public ListViewModel MainListModel
        {
            get { return _mainListModel; }
        }

        private void OpenSelected(string search)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];

            if (_selectedIndexables.Any())
            {
                var closedGenericType = GetInstanceOfGenericType(typeof(IAction<>), _selectedIndexables.Peek());

                if (closedGenericType != null && closedGenericType.GetGenericArguments()[0] == typeof(string))
                {
                    if (searchItemModel.TargetItem is StringIndexable)
                    {
                        var stringIndexable = searchItemModel.TargetItem as StringIndexable;
                        var action = (IAction<string>)_selectedIndexables.Peek();

                        _frecencyStorage.AddUse(action.BoostIdentifier, search, _mainListModel.SelectedIndex);

                        _stack.Clear();
                        _selectedIndexables.Clear();
                        _stack.Push(new IndexableSearchFrame(SearchResources.GetFiles().Concat(_actions).ToArray()));
                        StackPushed?.Invoke();

                        action.RunAction(stringIndexable.Name);
                        return;
                    }
                }
            }

            var standaloneAction = searchItemModel.TargetItem as IStandaloneAction;
            if (standaloneAction != null)
            {
                Log.Information("Launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, standaloneAction.GetType());

                try
                {
                    _frecencyStorage.AddUse(searchItemModel.TargetItem.BoostIdentifier, search, _mainListModel.SelectedIndex);
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

                _frecencyStorage.AddUse(searchItemModel.TargetItem.BoostIdentifier, search, _mainListModel.SelectedIndex);

                Log.Information("Launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, firstActionType);
                runMethod.Invoke(actionInstance, new[] { searchItemModel.TargetItem });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", searchItemModel.TargetItem, firstActionType);
            }
        }

        private void PushStack(string search)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];

            var genericActionType = typeof(IAction<>);

            var closedGenericType = GetInstanceOfGenericType(genericActionType, searchItemModel.TargetItem);

            if (closedGenericType != null)
            {
                _stack.Push(new StringSearchFrame());
                _selectedIndexables.Push(searchItemModel.TargetItem);
                StackPushed?.Invoke();
            }
        }

        static Type GetInstanceOfGenericType(Type genericType, object instance)
        {
            Type type = instance.GetType();

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return type;
                }

                foreach (var i in type.GetInterfaces())
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                    {
                        return i;
                    }
                }

                type = type.BaseType;
            }

            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BlockingCollection<IIntent> _queue;
        private Dispatcher _dispatcher;

        public void StartIntentService(Dispatcher dispatcher)
        {
            _queue = new BlockingCollection<IIntent>(new ConcurrentQueue<IIntent>());
            _dispatcher = dispatcher;

            var thread = new Thread(Process) { IsBackground = true };
            thread.Start();
        }

        public void AddIntent(IIntent intent)
        {
            _queue.Add(intent);
        }

        private void Process()
        {
            foreach (var intent in _queue.GetConsumingEnumerable())
            {
                if (intent is SearchIntent)
                {
                    var searchIntent = intent as SearchIntent;

                    var searchItemModels = _stack.Peek().PerformSearch(searchIntent.Search, _frecencyStorage);

                    _dispatcher.Invoke(() => UpdateSearchItems(searchItemModels));
                }
                else if (intent is ExecuteIntent)
                {
                    var executeIntent = intent as ExecuteIntent;

                    _dispatcher.Invoke(() =>
                    {
                        OpenSelected(executeIntent.Search);
                    });
                }
                else if (intent is MoveSelectionIntent)
                {
                    var moveSelectionIntent = intent as MoveSelectionIntent;

                    _dispatcher.Invoke(() =>
                    {
                        if (moveSelectionIntent.Direction == MoveDirection.Down)
                        {
                            MainListModel.SelectedIndex = Math.Min(MainListModel.Items.Count, MainListModel.SelectedIndex + 1);
                        }
                        else
                        {
                            MainListModel.SelectedIndex = Math.Max(0, MainListModel.SelectedIndex - 1);
                        }
                    });
                }
                else if (intent is PushStackIntent)
                {
                    var pushStackIntent = intent as PushStackIntent;

                    _dispatcher.Invoke(() =>
                    {
                        PushStack(pushStackIntent.Search);
                    });
                }
                else if (intent is ShutdownIntent)
                {
                    var shutdownIntent = intent as ShutdownIntent;
                    _dispatcher.Invoke(shutdownIntent.ShutdownAction);
                }
            }
        }
    }

    public interface ISearchFrame
    {
        SearchItemModel[] PerformSearch(string search, FrecencyStorage frecencyStorage);
    }

    public class IndexableSearchFrame : ISearchFrame
    {
        private Searcher _selectaSeacher;

        public IndexableSearchFrame(IIndexable[] indexables)
        {
            _selectaSeacher = Searcher.Create(indexables);
        }

        public SearchItemModel[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            var frecencyData = frecencyStorage.GetFrecencyData();
            Func<IIndexable, int> boosterFunc = x => frecencyData.ContainsKey(x.BoostIdentifier) ? frecencyData[x.BoostIdentifier] : 0;
            _selectaSeacher = _selectaSeacher.Search(search, boosterFunc);
            var searchResults = _selectaSeacher.SearchResults.Take(10);
            return searchResults.Select(x => new SearchItemModel(x.Name, x.Score, x.TargetItem, x.HighlightIndexes, x.TargetItem.GetIconResolver())).ToArray();
        }
    }

    public class StringSearchFrame : ISearchFrame
    {
        public SearchItemModel[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            return new[] { new SearchItemModel(search, 0, new StringIndexable(search), ImmutableHashSet.Create<int>(), null) };
        }
    }

    public class ExecuteIntent : IIntent
    {
        public string Search { get; set; }

        public ExecuteIntent(string search)
        {
            Search = search;
        }
    }

    public class PushStackIntent : IIntent
    {
        public string Search { get; set; }

        public PushStackIntent(string search)
        {
            Search = search;
        }
    }

    public class ShutdownIntent : IIntent
    {
        public Action ShutdownAction { get; }

        public ShutdownIntent(Action shutdownAction)
        {
            ShutdownAction = shutdownAction;
        }
    }

    public class MoveSelectionIntent : IIntent
    {
        public MoveDirection Direction { get; set; }

        public MoveSelectionIntent(MoveDirection direction)
        {
            Direction = direction;
        }
    }

    public enum MoveDirection
    {
        Up,
        Down
    }

    public class SearchIntent : IIntent
    {
        public string Search { get; set; }

        public SearchIntent(string search)
        {
            Search = search;
        }
    }

    public interface IIntent
    {

    }
}
