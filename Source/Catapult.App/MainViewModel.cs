using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using Catapult.Core;
using Catapult.Core.Actions;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Spotify;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Catapult.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ActionRegistry _actionRegistry;

        private readonly ListViewModel _mainListModel = new ListViewModel();
        private readonly FrecencyStorage _frecencyStorage;
        private readonly Stack<ISearchFrame> _stack = new Stack<ISearchFrame>();
        private readonly Stack<IIndexable> _selectedIndexables = new Stack<IIndexable>();

        public event Action StackPushed;

        public MainViewModel()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<ContainingFolderAction>();

            var frecencyPath = CatapultPaths.FrecencyPath;
            _frecencyStorage = new FrecencyStorage(frecencyPath);

            _actionRegistry.RegisterAction<SpotifyNextTrackAction>();
            _actionRegistry.RegisterAction<SpotifyPlayPauseAction>();
            _actionRegistry.RegisterAction<SpotifyPreviousTrackAction>();
            _actionRegistry.RegisterAction<SpotifyStopAction>();

            _actionRegistry.RegisterAction<KillProcessAction>();
            _actionRegistry.RegisterAction<OpenLastLogAction>();
            _actionRegistry.RegisterAction<OpenLogFolderAction>();
            _actionRegistry.RegisterAction<OpenConfigAction>();

            _actionRegistry.RegisterAction<GoogleAction>();
            _actionRegistry.RegisterAction<PathOfExileWikiAction>();
            _actionRegistry.RegisterAction<WikipediaAction>();

            Reset();

            StartIntentService(Dispatcher.CurrentDispatcher);

            StackPushed += MainViewModel_StackPushed;
        }

        private void MainViewModel_StackPushed()
        {
            ContextItems.Reset(_selectedIndexables.Select(x => x.Name));
        }

        public void Reset()
        {
            _stack.Clear();
            _selectedIndexables.Clear();
            _stack.Push(_actionRegistry.GetSearchFrame(null));
            StackPushed?.Invoke();
        }

        private void UpdateSearchItems(SearchItemModel[] searchItemModels)
        {
            MainListModel.Items.Reset(searchItemModels);
            MainListModel.SelectedIndex = 0;
        }

        public ListViewModel MainListModel => _mainListModel;

        public SmartObservableCollection<string> ContextItems { get; set; } = new SmartObservableCollection<string>();

        private void OpenSelected(string search)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];
            var targetItem = searchItemModel.TargetItem;

            if (_selectedIndexables.Any())
            {
                var lastSelectedIndexable = _selectedIndexables.Peek();

                IAction selectedAction = targetItem as IAction;
                IIndexable selectedIndexable = lastSelectedIndexable;

                if (selectedAction == null && lastSelectedIndexable is IAction)
                {
                    selectedAction = (IAction)lastSelectedIndexable;
                    selectedIndexable = targetItem;
                }

                if (selectedAction == null)
                {
                    throw new Exception("No action selected.");
                }

                var closedGenericType = GetInstanceOfGenericType(typeof(IAction<>), selectedAction);

                if (closedGenericType != null)
                {
                    if (selectedIndexable is StringIndexable)
                    {
                        var stringIndexable = selectedIndexable as StringIndexable;
                        var action = (IAction<StringIndexable>)selectedAction;

                        _frecencyStorage.AddUse(action.BoostIdentifier, search, _mainListModel.SelectedIndex);

                        Reset();

                        action.RunAction(stringIndexable);
                        return;
                    }

                    if (selectedIndexable is FileItem)
                    {
                        var fileItem = selectedIndexable as FileItem;
                        var action = (IAction<FileItem>)selectedAction;

                        _frecencyStorage.AddUse(action.BoostIdentifier, search, _mainListModel.SelectedIndex);

                        Reset();

                        action.RunAction(fileItem);
                        return;
                    }
                }
            }

            var standaloneAction = targetItem as IStandaloneAction;
            if (standaloneAction != null)
            {
                Log.Information("Launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());

                try
                {
                    _frecencyStorage.AddUse(targetItem.BoostIdentifier, search, _mainListModel.SelectedIndex);
                    standaloneAction.RunAction();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());
                }

                return;
            }

            var actionList = _actionRegistry.GetActionFor(targetItem.GetType());
            var firstActionType = actionList.First();

            try
            {
                var actionInstance = Activator.CreateInstance(firstActionType);
                var runMethod = firstActionType.GetMethod("RunAction");

                _frecencyStorage.AddUse(targetItem.BoostIdentifier, search, _mainListModel.SelectedIndex);

                Log.Information("Launching {@TargetItem} with {ActionType}", targetItem, firstActionType);
                runMethod.Invoke(actionInstance, new[] { targetItem });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", targetItem, firstActionType);
            }
        }

        private void PushStack(string search)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            SearchItemModel searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];
            Type itemType = searchItemModel.TargetItem.GetType();
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(itemType);

            if (searchFrame == null)
            {
                return;
            }
            
            _stack.Push(searchFrame);
            _selectedIndexables.Push(searchItemModel.TargetItem);
            StackPushed?.Invoke();
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

                    var searchResults = _stack.Peek().PerformSearch(searchIntent.Search, _frecencyStorage);

                    var itemModels = new List<SearchItemModel>();

                    var firstIndexable = _selectedIndexables.FirstOrDefault();
                    if (firstIndexable is GoogleAction && !string.IsNullOrWhiteSpace(searchIntent.Search))
                    {
                        using (var webClient = new WebClient())
                        {
                            var suggestionJson = webClient.DownloadString("http://suggestqueries.google.com/complete/search?client=firefox&q=" + Uri.EscapeDataString(searchIntent.Search));
                            var suggestions = (JArray)JsonConvert.DeserializeObject<object[]>(suggestionJson)[1];

                            foreach (var suggestion in suggestions.Children<JToken>().Select(x => x.ToString()).Except(new[] { searchIntent.Search }).Distinct())
                            {
                                itemModels.Add(new SearchItemModel(suggestion, 0, new StringIndexable(suggestion), ImmutableHashSet.Create<int>(), null));
                            }
                        }
                    }

                    _dispatcher.Invoke(() => UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).Concat(itemModels).ToArray()));
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
                        var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
                        UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).ToArray());
                    });
                }
                else if (intent is ShutdownIntent)
                {
                    var shutdownIntent = intent as ShutdownIntent;
                    _dispatcher.Invoke(shutdownIntent.ShutdownAction);
                }
                else if (intent is ClearIntent)
                {
                    _dispatcher.Invoke(Reset);
                }
            }
        }
    }

    public class ClearIntent : IIntent
    {

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
