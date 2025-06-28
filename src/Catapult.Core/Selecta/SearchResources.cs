using System.Diagnostics;
using Catapult.Core.Config;
using Catapult.Core.Indexes;
using Catapult.Core.Indexes.Extensions;
using Serilog;

namespace Catapult.Core.Selecta;

public static class SearchResources
{
    private static readonly object LockObject = new();
    private static DateTime _lastDelayedIndex = DateTime.MinValue;

    private static volatile FileItem[]? _files;
    private static volatile int _updateCounter = 1;

    private static string[] _paths;
    private static HashSet<string> _ignoredDirectories;
    private static ExtensionContainer _extensionContainer;

    public static void SetConfig(JsonUserConfiguration config)
    {
        _paths = config.Paths;
        _ignoredDirectories = [..config.IgnoredDirectories];
        _extensionContainer = new ExtensionContainer(config.Extensions.Select(x => new ExtensionInfo(x)));
    }

    private static FileItem[] GetFilesInternal()
    {
        lock (LockObject)
        {
            if (_files is not null)
            {
                EnqueueDelayedIndexing();
                return _files;
            }

            EnqueueDelayedIndexing();
            _files = BuildFileItems(_paths);
            Interlocked.Increment(ref _updateCounter);
        }

        return _files;
    }

    public static void EnqueueDelayedIndexing(bool isForced = false)
    {
        if (!isForced && DateTime.UtcNow - TimeSpan.FromMinutes(5) < _lastDelayedIndex)
        {
            return;
        }

        _lastDelayedIndex = DateTime.UtcNow;

        if (!_paths.Any())
        {
            return;
        }

        Log.Information($"Starting delayed indexing of: {string.Join(", ", _paths)}");

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                foreach (var indexPath in _paths)
                {
                    FileIndexStore.Instance.IndexDirectory(indexPath, _ignoredDirectories, _extensionContainer);
                    var fileItems = BuildFileItems(_paths);
                    lock (LockObject)
                    {
                        _files = fileItems;
                        Interlocked.Increment(ref _updateCounter);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Delayed indexing failed.");
            }
        });
    }

    private static FileItem[] BuildFileItems(string[] paths)
    {
        var stopwatch = Stopwatch.StartNew();

        var allFiles = paths.SelectMany(FileIndexStore.Instance.GetIndexedPaths);

        var fileItems = allFiles
            .Where(x => _extensionContainer.IsKnownExtension(Path.GetExtension(x)))
            .Distinct()
            .Select(BuildFileItem)
            .Where(x => x != null)
            .Cast<FileItem>()
            .ToArray();

        stopwatch.Stop();

        Log.Information("Built {count} FileItems for index in {time} ms", fileItems.Length, stopwatch.ElapsedMilliseconds);
        return fileItems;
    }

    private static FileItem? BuildFileItem(string fullName)
    {
        try
        {
            return new FileItem(fullName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to build FileItem for {FullName}", fullName);
            return null;
        }
    }

    public static int UpdateCounter => _updateCounter;

    public static FileItem[] GetFiles()
    {
        return GetFilesInternal();
    }
}