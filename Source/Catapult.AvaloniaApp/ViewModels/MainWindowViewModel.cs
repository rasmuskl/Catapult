using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Catapult.Core;
using Catapult.Core.Actions;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using ReactiveUI;
using Serilog;

namespace Catapult.AvaloniaApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ActionRegistry _actionRegistry;
        private readonly Stack<ISearchFrame> _stack = new Stack<ISearchFrame>();
        private readonly Stack<IIndexable> _selectedIndexables = new Stack<IIndexable>();

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        private int _searchBoxSelectedIndex;
        public int SearchBoxSelectedIndex
        {
            get => _searchBoxSelectedIndex;
            set => this.RaiseAndSetIfChanged(ref _searchBoxSelectedIndex, value);
        }

        private readonly ObservableAsPropertyHelper<IEnumerable<SearchItemModel>> _searchResults;
        public IEnumerable<SearchItemModel> SearchResults => _searchResults.Value;

        private readonly ObservableAsPropertyHelper<int> _targetHeight;
        public int TargetHeight => _targetHeight.Value;

        public MainWindowViewModel()
        {
            _actionRegistry = new ActionRegistry();

            //_actionRegistry.RegisterIndexer(() => new ControlPanelIndexer().GetControlPanelItems());

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<OpenAsAdminAction>();
            _actionRegistry.RegisterAction<ContainingFolderConverter>();

            var frecencyPath = CatapultPaths.FrecencyPath;
            _frecencyStorage = new FrecencyStorage(frecencyPath);

            //_actionRegistry.RegisterAction<SpotifyNextTrackAction>();
            //_actionRegistry.RegisterAction<SpotifyPlayPauseAction>();
            //_actionRegistry.RegisterAction<SpotifyPreviousTrackAction>();
            //_actionRegistry.RegisterAction<SpotifyStopAction>();

            _actionRegistry.RegisterAction<KillProcessAction>();
            _actionRegistry.RegisterAction<OpenLastLogAction>();
            _actionRegistry.RegisterAction<OpenLogFolderAction>();
            _actionRegistry.RegisterAction<OpenConfigAction>();
            //_actionRegistry.RegisterAction<EnableRunAtStartUpAction>();
            //_actionRegistry.RegisterAction<DisableRunAtStartUpAction>();
            _actionRegistry.RegisterAction<ReindexFilesAction>();

            //_actionRegistry.RegisterAction<CheckForUpdatesAction>();
            //CheckForUpdatesAction.UpdateCheckAction = SquirrelIntegration.Instance.CheckForUpdates;

            //SquirrelIntegration.OnUpdateFound += version =>
            //{
            //    _actionRegistry.RegisterAction<UpgradeCatapultAction>();
            //    UpgradeCatapultAction.UpgradeAction = SquirrelIntegration.Instance.UpgradeToNewVersion;
            //};

            _actionRegistry.RegisterAction<GoogleAction>();
            _actionRegistry.RegisterAction<WikipediaAction>();

            //_actionRegistry.RegisterAction<ClipboardHistoryAction>();
            _actionRegistry.RegisterAction<UnderscorizeClipboardString>();
            _actionRegistry.RegisterAction<TripleBacktickClipboardString>();

            //_actionRegistry.RegisterAction<WindowsSleepAction>();
            //_actionRegistry.RegisterAction<WindowsRestartAction>();
            //_actionRegistry.RegisterAction<WindowsShutdownAction>();
            //_actionRegistry.RegisterAction<WindowsShutdownForceAction>();
            //_actionRegistry.RegisterAction<WindowsLockComputerAction>();
            //_actionRegistry.RegisterAction<WindowsLogOffAction>();

            _searchResults = this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(term => term?.Trim())
                .DistinctUntilChanged()
                .SelectMany(Search)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.SearchResults);

            _targetHeight = this.WhenAnyValue(x => x.SearchResults)
                .Select(x => 75 + Math.Min(x?.Count() * 50 ?? 0, 300))
                .ToProperty(this, x => x.TargetHeight);

            StartIntentService(Dispatcher.UIThread);

            Reset();
        }

        private async Task<IEnumerable<SearchItemModel>> Search(string term, CancellationToken token)
        {
            if (!_stack.Any())
            {
                return Array.Empty<SearchItemModel>();
            }

            var searchResultViewModels = _stack.Peek()
                .PerformSearch(term, _frecencyStorage)
                .Select(x => new SearchItemModel(x))
                .ToArray();

            return searchResultViewModels;
        }

        private BlockingCollection<IIntent> _queue;
        private Dispatcher _dispatcher;
        private readonly FrecencyStorage _frecencyStorage;

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
                try
                {
                    if (intent is ExecuteIntent)
                    {
                        var executeIntent = intent as ExecuteIntent;
                        _dispatcher.Post(() => { OpenSelected(executeIntent.Search, executeIntent.AfterOpenAction); });
                    }
                    else if (intent is MoveSelectionIntent)
                    {
                        var moveSelectionIntent = intent as MoveSelectionIntent;

                        _dispatcher.Post(() =>
                        {
                            if (moveSelectionIntent.Direction == MoveDirection.Down)
                            {
                                SearchBoxSelectedIndex = Math.Min(SearchResults.Count(), SearchBoxSelectedIndex + moveSelectionIntent.Count);
                            }
                            else if (moveSelectionIntent.Direction == MoveDirection.Up)
                            {
                                SearchBoxSelectedIndex = Math.Max(0, SearchBoxSelectedIndex - moveSelectionIntent.Count);
                            }
                            else if (moveSelectionIntent.Direction == MoveDirection.SetIndex)
                            {
                                SearchBoxSelectedIndex = Math.Min(Math.Max(0, moveSelectionIntent.Count), SearchResults.Count());
                            }
                        });
                    }
                    else if (intent is PushStackIntent)
                    {
                        _dispatcher.Post(() =>
                        {
                            if (!PushStack())
                            {
                                return;
                            }

                            SearchTerm = string.Empty;
                            //var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
                            //UpdateSearchItems(searchResults);
                        });
                    }
                    else if (intent is FastActionIntent)
                    {
                        var fastActionIntent = intent as FastActionIntent;
                        //_dispatcher.Post(() => { PerformFastAction(fastActionIntent.FastAction); });
                    }
                    else if (intent is ShutdownIntent)
                    {
                        var shutdownIntent = intent as ShutdownIntent;
                        _dispatcher.Post(shutdownIntent.ShutdownAction);
                    }
                    else if (intent is ClearIntent)
                    {
                        _dispatcher.Post(Reset);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Process failed while executing {intent}", intent);
#if DEBUG
                    throw;
#endif
                }
            }
        }

        private void Reset()
        {
            _stack.Clear();
            _selectedIndexables.Clear();
            _stack.Push(_actionRegistry.GetSearchFrame(null));
            //StackPushed?.Invoke();
            SearchTerm = string.Empty;
        }

        private void OpenSelected(string search, Action afterOpenAction)
        {
            if (!SearchResults.Any())
            {
                return;
            }

            var searchItemModel = SearchResults.ElementAt(SearchBoxSelectedIndex);
            var targetItem = searchItemModel.TargetItem;

            if (targetItem is IStandaloneAction standaloneAction)
            {
                Log.Information("Launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());

                try
                {
                    _frecencyStorage.AddUse(targetItem.BoostIdentifier, search, SearchBoxSelectedIndex);
                    standaloneAction.Run();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());
                }

                afterOpenAction();
                return;
            }

            var launchable = _actionRegistry.Launch(_selectedIndexables.Reverse().Concat(new[] { targetItem }).ToArray());

            if (launchable?.Action == null)
            {
                throw new Exception("Unable to find action.");
            }

            if (launchable.Target == null)
            {
                if (PushStack())
                {
                    return;
                }

                throw new Exception("Unable to find target.");
            }

            try
            {
                _frecencyStorage.AddUse(launchable.Action.BoostIdentifier, search, SearchBoxSelectedIndex);
                _frecencyStorage.AddUse(launchable.Target.BoostIdentifier, search, SearchBoxSelectedIndex);


                Log.Information("Launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);

                launchable.Action.GetType()
                    .GetMethod("Run", new[] { launchable.Target.GetType() })
                    .Invoke(launchable.Action, new[] { launchable.Target });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);
            }

            afterOpenAction();
            Reset();
        }


        private bool PushStack()
        {
            if (!SearchResults.Any())
            {
                return false;
            }

            SearchItemModel searchItemModel = SearchResults.ElementAt(SearchBoxSelectedIndex);
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(_selectedIndexables.Reverse().Concat(new[] { searchItemModel.TargetItem }).ToArray());

            if (searchFrame == null)
            {
                return false;
            }

            _stack.Push(searchFrame);
            _selectedIndexables.Push(searchItemModel.TargetItem);
            //StackPushed?.Invoke();
            return true;
        }


        //private void UpdateSearchItems(SearchResult[] searchResults)
        //{
        //    var oldItems = SearchResults.ToArray();

        //    var searchItemModels = searchResults.Select(x => new SearchResultViewModel()).ToArray();

        //    foreach (SearchResultViewModel searchItemModel in searchItemModels)
        //    {
        //        //IconService.Instance.Enqueue(new IconRequest(searchItemModel));
        //    }

        //    //MainListModel.Items.Reset(searchItemModels);
        //    //MainListModel.SelectedIndex = 0;

        //    foreach (var oldItem in oldItems)
        //    {
        //        oldItem.Dispose();
        //    }
        //}
    }

    public class ClearIntent : IIntent
    {
    }

    public class ExecuteIntent : IIntent
    {
        public string Search { get; set; }
        public Action AfterOpenAction { get; set; }

        public ExecuteIntent(string search, Action afterOpenAction)
        {
            Search = search;
            AfterOpenAction = afterOpenAction;
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

    public class FastActionIntent : IIntent
    {
        public FastAction FastAction { get; set; }

        public FastActionIntent(FastAction fastAction)
        {
            FastAction = fastAction;
        }
    }

    public enum FastAction
    {
        Left,
        Right
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
        public int Count { get; set; }

        public MoveSelectionIntent(MoveDirection direction, int count = 1)
        {
            Direction = direction;
            Count = count;
        }
    }

    public enum MoveDirection
    {
        Up,
        Down,
        SetIndex
    }

    public interface IIntent
    {
    }

    public class SearchItemModel : INotifyPropertyChanged, IDisposable
    {
        //private BitmapSource _icon;

        public SearchItemModel(string name, string details, double score, IIndexable targetItem, ImmutableHashSet<int> highlightIndexes)
        {
            Name = name;
            Details = details;
            Score = score;
            TargetItem = targetItem;
            HighlightIndexes = highlightIndexes;
        }

        public SearchItemModel(SearchResult result) : this(result.Name, result.TargetItem.Details, result.Score, result.TargetItem, result.HighlightIndexes)
        {
        }

        public string Name { get; set; }
        public string Details { get; set; }

        public double Score { get; set; }

        public IIndexable TargetItem { get; set; }

        public Guid Id { get; set; }

        public ImmutableHashSet<int> HighlightIndexes { get; set; }

        //public BitmapSource Icon
        //{
        //    get { return _icon; }
        //    set
        //    {
        //        _icon = value;
        //        OnPropertyChanged();
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed { get; private set; }
    }

    public class IconRequest
    {
        public SearchItemModel Model { get; }

        public IconRequest(SearchItemModel model)
        {
            Model = model;
        }
    }
}
