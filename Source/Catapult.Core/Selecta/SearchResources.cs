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
        private static readonly object DelayedIndexLock = new object();

        private static volatile FileItem[] _files;
        private static DateTime _lastUpdatedUtc;

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

                var toBeIndexed = new List<string>();

                foreach (var path in _paths)
                {
                    var indexedPaths = FileIndexStore.Instance.IsIndexed(path);

                    if (!indexedPaths)
                    {
                        FileIndexStore.Instance.IndexDirectory(path, _ignoredDirectories, _extensionContainer);
                    }
                    else
                    {
                        toBeIndexed.Add(path);
                    }
                }

                _files = BuildFileItems(_paths);
                _lastUpdatedUtc = DateTime.UtcNow;
                EnqueueDelayedIndexing(toBeIndexed);
            }

            return _files;
        }


        private static void EnqueueDelayedIndexing(List<string> toBeIndexed = null)
        {
            lock (DelayedIndexLock)
            {
                toBeIndexed = toBeIndexed ?? _paths.Where(x => !FileIndexStore.Instance.IsIndexed(x, TimeSpan.FromMinutes(5))).ToList();

                if (!toBeIndexed.Any())
                {
                    return;
                }

                Log.Information("Starting delayed indexing of " + string.Join(", ", toBeIndexed));

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        foreach (var indexPath in toBeIndexed)
                        {
                            FileIndexStore.Instance.IndexDirectory(indexPath, _ignoredDirectories, _extensionContainer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Delayed indexing failed.", ex);
                    }

                    lock (LockObject)
                    {
                        _files = BuildFileItems(_paths);
                        _lastUpdatedUtc = DateTime.UtcNow;
                        Log.Information("Delayed indexing complete.");
                    }
                });
            }
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

        public static DateTime LastUpdatedUtc => _lastUpdatedUtc;

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