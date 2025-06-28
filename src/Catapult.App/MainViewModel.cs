using System.Collections.Concurrent;
using System.IO;
using System.Windows.Threading;
using Catapult.App.Core;
using Catapult.App.Core.Actions;
using Catapult.Core;
using Catapult.Core.Actions;
using Catapult.Core.Frecency;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;
using Serilog;

namespace Catapult.App;

public sealed class MainViewModel
{
    private readonly ActionRegistry _actionRegistry;

    private readonly ListViewModel _mainListModel = new();
    private readonly FrecencyStorage _frecencyStorage;
    private readonly Stack<ISearchFrame> _stack = new();
    private readonly Stack<IIndexable> _selectedIndexables = new();

    public event Action? StackPushed;

    public MainViewModel()
    {
        _actionRegistry = new ActionRegistry();

        _actionRegistry.RegisterIndexer(() => new ControlPanelIndexer().GetControlPanelItems());
        _actionRegistry.RegisterIndexer(() => new WindowsStoreAppIndexer().GetWindowsStoreApps());

        _actionRegistry.RegisterAction<OpenAction>();
        _actionRegistry.RegisterAction<OpenAsAdminAction>();
        _actionRegistry.RegisterAction<ContainingFolderConverter>();

        _actionRegistry.RegisterAction<WindowsStoreAppOpenAction>();

        var frecencyPath = CatapultPaths.FrecencyPath;
        _frecencyStorage = new FrecencyStorage(frecencyPath);

        _actionRegistry.RegisterAction<KillProcessAction>();
        _actionRegistry.RegisterAction<OpenLastLogAction>();
        _actionRegistry.RegisterAction<OpenLogFolderAction>();
        _actionRegistry.RegisterAction<OpenConfigAction>();
        _actionRegistry.RegisterAction<EnableRunAtStartUpAction>();
        _actionRegistry.RegisterAction<DisableRunAtStartUpAction>();
        _actionRegistry.RegisterAction<ReindexFilesAction>();
        _actionRegistry.RegisterAction<CheckForUpdatesAction>();

        _actionRegistry.RegisterAction<GoogleAction>();
        _actionRegistry.RegisterAction<WikipediaAction>();

        _actionRegistry.RegisterAction<UnderscorizeClipboardString>();
        _actionRegistry.RegisterAction<TripleBacktickClipboardString>();

        _actionRegistry.RegisterAction<WindowsSleepAction>();
        _actionRegistry.RegisterAction<WindowsRestartAction>();
        _actionRegistry.RegisterAction<WindowsShutdownAction>();
        _actionRegistry.RegisterAction<WindowsShutdownForceAction>();
        _actionRegistry.RegisterAction<WindowsLockComputerAction>();
        _actionRegistry.RegisterAction<WindowsLogOffAction>();

        FileIconResolver.ResolverFunc = WindowsFileIconResolver.Resolve;

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
        _stack.Push(_actionRegistry.GetSearchFrame([]) ?? throw new InvalidOperationException("Unable to find search frame."));
        StackPushed?.Invoke();

        var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
        UpdateSearchItems(searchResults);
    }

    private void UpdateSearchItems(SearchResult[] searchResults)
    {
        var oldItems = MainListModel.Items.ToArray();

        var searchItemModels = searchResults.Select(x => new SearchItemModel(x)).ToArray();

        foreach (SearchItemModel searchItemModel in searchItemModels)
        {
            IconService.Instance.Enqueue(new IconRequest(searchItemModel));
        }

        MainListModel.Items.Reset(searchItemModels);
        MainListModel.SelectedIndex = 0;

        foreach (SearchItemModel oldItem in oldItems)
        {
            oldItem.Dispose();
        }
    }

    public ListViewModel MainListModel => _mainListModel;

    public SmartObservableCollection<string> ContextItems { get; } = new();

    private void OpenSelected(string search, Action afterOpenAction)
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

            afterOpenAction();
            return;
        }

        var launchable = _actionRegistry.Launch(_selectedIndexables.Reverse().Concat([targetItem]).ToArray());

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
            _frecencyStorage.AddUse(launchable.Action.BoostIdentifier, search, _mainListModel.SelectedIndex);
            _frecencyStorage.AddUse(launchable.Target.BoostIdentifier, search, _mainListModel.SelectedIndex);


            Log.Information("Launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);

            launchable.Action.GetType()
                .GetMethod("Run", new[] {launchable.Target.GetType()})
                .Invoke(launchable.Action, new[] {launchable.Target});
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception launching {@TargetItem} with {ActionType}", launchable.Target, launchable.Action);
        }

        afterOpenAction();
        Reset();
    }

    private void PerformFastAction(FastAction fastAction)
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
            UpdateSearchItems(searchResults);
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
            UpdateSearchItems(searchResults);
        }
    }

    private bool PushStack()
    {
        if (!_mainListModel.Items.Any())
        {
            return false;
        }

        SearchItemModel searchItemModel = _mainListModel.Items[_mainListModel.SelectedIndex];
        ISearchFrame searchFrame = _actionRegistry.GetSearchFrame(_selectedIndexables.Reverse().Concat(new[] {searchItemModel.TargetItem}).ToArray());

        if (searchFrame == null)
        {
            return false;
        }

        _stack.Push(searchFrame);
        _selectedIndexables.Push(searchItemModel.TargetItem);
        StackPushed?.Invoke();
        return true;
    }

    private BlockingCollection<IIntent> _queue;
    private Dispatcher _dispatcher;

    private void StartIntentService(Dispatcher dispatcher)
    {
        _queue = new BlockingCollection<IIntent>(new ConcurrentQueue<IIntent>());
        _dispatcher = dispatcher;

        var thread = new Thread(Process) {IsBackground = true};
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
                if (intent is SearchIntent searchIntent)
                {
                    var searchResults = _stack.Peek().PerformSearch(searchIntent.Search, _frecencyStorage);

                    _dispatcher.Invoke(() => UpdateSearchItems(searchResults));
                }
                else if (intent is ExecuteIntent executeIntent)
                {
                    _dispatcher.Invoke(() => { OpenSelected(executeIntent.Search, executeIntent.AfterOpenAction); });
                }
                else if (intent is MoveSelectionIntent moveSelectionIntent)
                {
                    _dispatcher.Invoke(() =>
                    {
                        if (moveSelectionIntent.Direction == MoveDirection.Down)
                        {
                            MainListModel.SelectedIndex = Math.Min(MainListModel.Items.Count, MainListModel.SelectedIndex + moveSelectionIntent.Count);
                        }
                        else if (moveSelectionIntent.Direction == MoveDirection.Up)
                        {
                            MainListModel.SelectedIndex = Math.Max(0, MainListModel.SelectedIndex - moveSelectionIntent.Count);
                        }
                        else if (moveSelectionIntent.Direction == MoveDirection.SetIndex)
                        {
                            MainListModel.SelectedIndex = Math.Min(Math.Max(0, moveSelectionIntent.Count), MainListModel.Items.Count);
                        }
                    });
                }
                else if (intent is PushStackIntent)
                {
                    _dispatcher.Invoke(() =>
                    {
                        if (!PushStack())
                        {
                            return;
                        }

                        var searchResults = _stack.Peek().PerformSearch(string.Empty, _frecencyStorage);
                        UpdateSearchItems(searchResults);
                    });
                }
                else if (intent is FastActionIntent fastActionIntent)
                {
                    _dispatcher.Invoke(() => { PerformFastAction(fastActionIntent.FastAction); });
                }
                else if (intent is ShutdownIntent shutdownIntent)
                {
                    _dispatcher.Invoke(shutdownIntent.ShutdownAction);
                }
                else if (intent is ClearIntent)
                {
                    _dispatcher.Invoke(Reset);
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
}

public class ClearIntent : IIntent
{
}

public class ExecuteIntent(string search, Action afterOpenAction) : IIntent
{
    public string Search { get; set; } = search;
    public Action AfterOpenAction { get; set; } = afterOpenAction;
}

public class PushStackIntent(string search) : IIntent
{
    public string Search { get; set; } = search;
}

public class FastActionIntent(FastAction fastAction) : IIntent
{
    public FastAction FastAction { get; set; } = fastAction;
}

public enum FastAction
{
    Left,
    Right
}

public class ShutdownIntent(Action shutdownAction) : IIntent
{
    public Action ShutdownAction { get; } = shutdownAction;
}

public class MoveSelectionIntent(MoveDirection direction, int count = 1) : IIntent
{
    public MoveDirection Direction { get; set; } = direction;
    public int Count { get; set; } = count;
}

public enum MoveDirection
{
    Up,
    Down,
    SetIndex
}

public class SearchIntent(string search) : IIntent
{
    public string Search { get; set; } = search;
}

public interface IIntent
{
}