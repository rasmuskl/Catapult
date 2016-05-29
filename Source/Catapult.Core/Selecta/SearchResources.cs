using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Catapult.Core.Config;
using Catapult.Core.Indexes;
using Catapult.Core.Indexes.Extensions;
using Serilog;

namespace Catapult.Core.Selecta
{
    public static class SearchResources
    {
        private static readonly object LockObject = new object();
        private static DateTime _lastDelayedIndex = DateTime.MinValue;

        private static volatile FileItem[] _files;
        private static volatile int _updateCounter = 1;

        private static string[] _paths;
        private static HashSet<string> _ignoredDirectories;
        private static ExtensionContainer _extensionContainer;
        private static ClipboardIndexer _clipboardIndexer;

        public static void SetConfig(JsonUserConfiguration config)
        {
            _paths = config.Paths;
            _ignoredDirectories = new HashSet<string>(config.IgnoredDirectories);
            _extensionContainer = new ExtensionContainer(config.Extensions.Select(x => new ExtensionInfo(x)));

            _clipboardIndexer = new ClipboardIndexer();
        }

        private static FileItem[] GetFilesInternal()
        {
            lock (LockObject)
            {
                if (_files != null)
                {
                    EnqueueDelayedIndexing();
                    return _files;
                }

                EnqueueDelayedIndexing();
                _files = BuildFileItems(_paths);
                _updateCounter += 1;
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

            Log.Information("Starting delayed indexing of " + string.Join(", ", _paths));

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    foreach (var indexPath in _paths)
                    {
                        FileIndexStore.Instance.IndexDirectory(indexPath, _ignoredDirectories, _extensionContainer);
                        lock (LockObject)
                        {
                            _files = BuildFileItems(_paths);
                            _updateCounter += 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Delayed indexing failed.", ex);
                }
            });
        }

        private static FileItem[] BuildFileItems(string[] paths)
        {
            var stopwatch = Stopwatch.StartNew();

            var allFiles = paths.SelectMany(x => FileIndexStore.Instance.GetIndexedPaths(x));

            var fileItems = allFiles
                .Where(x => _extensionContainer.IsKnownExtension(Path.GetExtension(x)))
                .Distinct()
                .Select(BuildFileItem)
                .Where(x => x != null)
                .ToArray();

            stopwatch.Stop();

            Log.Information("Built {count} FileItems for index in {time} ms", fileItems.Length, stopwatch.ElapsedMilliseconds);
            return fileItems;
        }

        private static FileItem BuildFileItem(string fullName)
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

        public static ClipboardEntry[] GetClipboardHistory()
        {
            return _clipboardIndexer.ClipboardEntries
                .OrderByDescending(x => x.CreatedUtc)
                .ToArray();
        }

        public static void Dispose()
        {
            _clipboardIndexer?.Dispose();
        }
    }
}