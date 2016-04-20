using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Catapult.Core.Config;
using Catapult.Core.Indexes.Extensions;

namespace Catapult.Core.Selecta
{
    public static class SearchResources
    {
        private static readonly object LockObject = new object();
        private static FileItem[] _files;

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

                var allFiles = _paths.SelectMany(x => SafeWalk.EnumerateFiles(x, _ignoredDirectories));
                var fileItems = allFiles
                    .Where(x => _extensionContainer.IsKnownExtension(Path.GetExtension(x)))
                    .Distinct()
                    .Select(x => new FileItem(x))
                    .ToArray();

                stopwatch.Stop();

                //Log.Info($"Traversed {files.Length} files in {stopwatch.ElapsedMilliseconds} ms");

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