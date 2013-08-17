using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using AlphaLaunch.Core.Config;
using AlphaLaunch.Core.Debug;

namespace AlphaLaunch.Core.Indexes
{
    public class IndexStore
    {
        public static readonly IndexStore Instance = new IndexStore();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ISearcher _searcher = new FuzzySearcher();

        private IndexStore()
        {
        }

        public void Start()
        {
            IndexDirectory("Start menu", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            IndexDirectory("Common start menu", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

            var loader = new JsonConfigLoader();
            var config = loader.Load("config.json");
            loader.Save(config, "config.json");

            foreach (var path in config.Paths)
            {
                IndexDirectory(path, path);
            }
        }

        private void IndexDirectory(string name, string path)
        {
            var stopwatch = Stopwatch.StartNew();

            var directory = new DirectoryInfo(path);
            var fileItems = GetFiles(directory).ToArray();

            try
            {
                _lock.EnterWriteLock();
                _searcher.IndexItems(fileItems);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            stopwatch.Stop();

            Log.Info("Indexed " + name + " - " + fileItems.Length + " items. [" + stopwatch.ElapsedMilliseconds + " ms]");
        }

        private IEnumerable<FileItem> GetFiles(DirectoryInfo directory)
        { 
            return directory.GetFiles().Select(x => new FileItem(x.FullName))
                .Concat(directory.GetDirectories().SelectMany(GetFiles));
        }

        public IEnumerable<SearchResult> Search(string search)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = _searcher.Search(search);
            stopwatch.Stop();
            Log.Info("Found " + results.Count() + " results. [" + stopwatch.ElapsedMilliseconds + " ms]");

            return results;
        }
    }
}