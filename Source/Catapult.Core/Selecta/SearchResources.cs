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
        private static volatile FileItem[] _files;

        private static string[] _paths;
        private static HashSet<string> _ignoredDirectories;
        private static ExtensionContainer _extensionContainer;

        public static void SetConfig(JsonUserConfiguration config)
        {
            _paths = config.Paths;
            _ignoredDirectories = new HashSet<string>(config.IgnoredDirectories);
            _extensionContainer = new ExtensionContainer(config.Extensions.Select(x => new ExtensionInfo(x)));
        }

        private static FileItem[] GetFilesInternal()
        {
            lock (LockObject)
            {
                if (_files != null)
                {
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

                var fileItems = BuildFileItems(_paths);

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
                        Log.Information("Delayed indexing complete.");
                    }
                });

                _files = fileItems;
            }

            return _files;
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

        public static FileItem[] GetFiles()
        {
            return GetFilesInternal();
        }
    }
}