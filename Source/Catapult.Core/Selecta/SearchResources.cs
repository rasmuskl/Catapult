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
            if (_files != null)
            {
                return _files;
            }

            lock (LockObject)
            {
                if (_files != null)
                {
                    return _files;
                }

                var stopwatch = Stopwatch.StartNew();

                var allFiles = new List<string>();
                var toBeIndexed = new List<string>();

                foreach (var path in _paths)
                {
                    var indexedPaths = FileIndexStore.Instance.GetIndexedPaths(path);

                    if (indexedPaths == null)
                    {
                        FileIndexStore.Instance.IndexDirectory(path, _ignoredDirectories, _extensionContainer);
                        indexedPaths = FileIndexStore.Instance.GetIndexedPaths(path);
                    }
                    else
                    {
                        toBeIndexed.Add(path);
                    }

                    allFiles.AddRange(indexedPaths);
                }

                var fileItems = allFiles
                    .Where(x => _extensionContainer.IsKnownExtension(Path.GetExtension(x)))
                    .Distinct()
                    .Select(x => new FileItem(x))
                    .ToArray();

                stopwatch.Stop();

                //Log.Info($"Traversed {files.Length} files in {stopwatch.ElapsedMilliseconds} ms");

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
                });

                _files = fileItems;
            }

            return _files;
        }

        public static FileItem[] GetFiles()
        {
            return GetFilesInternal();
        }
    }
}