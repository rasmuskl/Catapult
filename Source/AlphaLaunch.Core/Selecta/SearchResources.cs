using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AlphaLaunch.Core.Indexes.Extensions;

namespace AlphaLaunch.Core.Selecta
{
    public static class SearchResources
    {
        private static readonly Lazy<FileItem[]> _fileFunc = new Lazy<FileItem[]>(GetFilesInternal, LazyThreadSafetyMode.ExecutionAndPublication);

        private static FileItem[] GetFilesInternal()
        {
            var stopwatch = Stopwatch.StartNew();

            var paths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                //Environment.GetFolderPath(Environment.SpecialFolder.Recent),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                @"c:\dev",
            };


            var ignoredDirectories = new HashSet<string>(new[] { "node_modules", ".git", "scratch" });

            var extensionContainer = new ExtensionContainer(new[] { new ExtensionInfo(".lnk"), new ExtensionInfo(".exe"), new ExtensionInfo(".sln"), new ExtensionInfo(".url"), });

            var allFiles = paths.SelectMany(x => SafeWalk.EnumerateFiles(x, ignoredDirectories));
            var fileItems = allFiles
                .Where(x => extensionContainer.IsKnownExtension(Path.GetExtension(x)))
                .Select(x => new FileItem(x))
                .ToArray();

            stopwatch.Stop();

            //Log.Info($"Traversed {files.Length} files in {stopwatch.ElapsedMilliseconds} ms");

            return fileItems;
        }

        public static FileItem[] GetFiles()
        {
            return _fileFunc.Value;
        }
    }
}