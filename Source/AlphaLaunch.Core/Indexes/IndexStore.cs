using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Config;
using AlphaLaunch.Core.Debug;

namespace AlphaLaunch.Core.Indexes
{
    public class IndexStore
    {
        public static readonly IndexStore Instance = new IndexStore();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ISearcher _searcher = new FuzzySearcher();
        private JsonIndexData _indexData;
        private readonly JsonConfigLoader _loader = new JsonConfigLoader();
        private ImmutableDictionary<string, EntryBoost> _boostEntries;
        private const string IndexJsonPath = "index.json";
        private const string ConfigJsonPath = "config.json";

        private IndexStore()
        {
        }

        public void Start()
        {
            IndexDirectory("Start menu", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            IndexDirectory("Common start menu", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            IndexDirectory("Favorites", Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            IndexDirectory("Recent", Environment.GetFolderPath(Environment.SpecialFolder.Recent));
            IndexDirectory("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            var config = _loader.LoadUserConfig(ConfigJsonPath);
            _loader.SaveUserConfig(config, ConfigJsonPath);

            _indexData = _loader.LoadIndexData(IndexJsonPath);
            _boostEntries = _indexData.BoostEntries.ToImmutableDictionary();

            foreach (var path in config.Paths)
            {
                IndexDirectory(path, path);
            }
        }

        public void IndexAction(IStandaloneAction standaloneAction)
        {
            try
            {
                _lock.EnterWriteLock();
                _searcher.IndexItems(new[] { standaloneAction });
            }
            finally
            {
                _lock.ExitWriteLock();
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

        public void AddBoost(string searchString, string boostIdentifier)
        {
            _boostEntries = _boostEntries.SetItem(searchString, new EntryBoost(boostIdentifier));

            _indexData.BoostEntries = _boostEntries.ToDictionary(x => x.Key, x => x.Value);
            _loader.SaveIndexData(_indexData, IndexJsonPath);
        }

        public IEnumerable<SearchResult> Search(string search)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = _searcher.Search(search, _boostEntries);
            stopwatch.Stop();
            Log.Info("Found " + results.Count() + " results. [" + stopwatch.ElapsedMilliseconds + " ms]");

            return results;
        }
    }
}