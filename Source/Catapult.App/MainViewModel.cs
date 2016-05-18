using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Catapult.Core;
using Catapult.Core.Actions;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Spotify;
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
            _actionRegistry.RegisterAction<ContainingFolderConverter>();

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
            _actionRegistry.RegisterAction<EnableRunAtStartUpAction>();
            _actionRegistry.RegisterAction<DisableRunAtStartUpAction>();

            _actionRegistry.RegisterAction<GoogleAction>();
            _actionRegistry.RegisterAction<PathOfExileWikiAction>();
            _actionRegistry.RegisterAction<WikipediaAction>();

            _actionRegistry.RegisterAction<UnderscorizeClipboardString>();

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

            var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
            UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).Take(10).ToArray());
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
            var standaloneAction = targetItem as IStandaloneAction;

            if (standaloneAction != null)
            {
                Log.Information("Launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());

                try
                {
                    _frecencyStorage.AddUse(targetItem.BoostIdentifier, search, _mainListModel.SelectedIndex);
                    standaloneAction.Run();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", targetItem, standaloneAction.GetType());
                }

                return;
            }

            var launchable = _actionRegistry.Launch(_selectedIndexables.Reverse().Concat(new[] { targetItem }).ToArray());

            if (launchable?.Action == null)
            {
                throw new Exception("Unable to find action.");
            }

            if (launchable.Target == null)
            {
                throw new Exception("Unable to find target.");
            }

            try
            {
                _frecencyStorage.AddUse(launchable.Action.BoostIdentifier, search, _mainListModel.SelectedIndex);
                _frecencyStorage.AddUse(launchable.Target.BoostIdentifier, search, _mainListModel.SelectedIndex);

                Reset();

                Log.Information("Launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);

                launchable.Action.GetType()
                    .GetMethod("Run", new[] { launchable.Target.GetType() })
                    .Invoke(launchable.Action, new[] { launchable.Target });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);
            }
        }

        public void PerformFastAction(FastAction fastAction)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];
            var targetItem = searchItemModel.TargetItem;

            var fileItem = targetItem as FileItem;
            var folderItem = targetItem as FolderItem;

            var path = fileItem?.FullName ?? folderItem?.FullName;

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (fastAction == FastAction.Left)
            {
                var parentPath = Path.GetDirectoryName(path);
                
                if (_stack.Peek() is FileNavigationSearchFrame)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(parentPath)))
                    {
                        return;
                    }

                    _stack.Push(new FileNavigationSearchFrame(Path.GetDirectoryName(parentPath), parentPath));
                }
                else
                {
                    if (!Directory.Exists(parentPath))
                    {
                        return;
                    }

                    _stack.Push(new FileNavigationSearchFrame(parentPath, path));
                }

                StackPushed?.Invoke();

                var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
                UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).Take(10).ToArray());
            }
            else if (fastAction == FastAction.Right)
            {
                if (folderItem == null)
                {
                    return;
                }

                _stack.Push(new FileNavigationSearchFrame(folderItem.FullName, null));
                StackPushed?.Invoke();

                var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
                UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).Take(10).ToArray());
            }
        }

        private void PushStack(string search)
        {
            if (!_mainListModel.Items.Any())
            {
                return;
            }

            SearchItemModel searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];
            ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(_selectedIndexables.Reverse().Concat(new[] { searchItemModel.TargetItem }).ToArray());

            if (searchFrame == null)
            {
                return;
            }

            _stack.Push(searchFrame);
            _selectedIndexables.Push(searchItemModel.TargetItem);
            StackPushed?.Invoke();
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

                    _dispatcher.Invoke(() => UpdateSearchItems(searchResults.Select(x => new SearchItemModel(x)).ToArray()));
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
                else if (intent is FastActionIntent)
                {
                    var fastActionIntent = intent as FastActionIntent;

                    _dispatcher.Invoke(() =>
                    {
                        PerformFastAction(fastActionIntent.FastAction);
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
