using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Catapult.Core.Config;
using Serilog;

namespace Catapult.Core.Indexes
{
    public class IndexStore
    {
        public static readonly IndexStore Instance = new IndexStore();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly JsonConfigLoader _loader = new JsonConfigLoader();
        private const string ConfigJsonPath = "config.json";
        private readonly DirectoryTraverser _directoryTraverser = new DirectoryTraverser();
        private readonly SearchIndex _searchIndex = new SearchIndex();

        private IndexStore()
        {
        }

        public void Start()
        {
            IndexDirectory("Start menu", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            IndexDirectory("Common start menu", Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            IndexDirectory("Favorites", Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            IndexDirectory("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            IndexDirectory("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            var config = _loader.LoadUserConfig(ConfigJsonPath);
            _loader.SaveUserConfig(config, ConfigJsonPath);

            foreach (var path in config.Paths)
            {
                IndexDirectory(path, path);
            }
        }

        public void IndexAction(IIndexable action)
        {
            try
            {
                _lock.EnterWriteLock();
                _searchIndex.AppendToIndex(action.BoostIdentifier, new[] { action });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void IndexDirectory(string name, string path)
        {
            var traverseDirectoriesWatch = Stopwatch.StartNew();

            var directory = new DirectoryInfo(path);
            var fileItems = _directoryTraverser.GetFiles(directory);

            traverseDirectoriesWatch.Stop();

            var indexWatch = Stopwatch.StartNew();

            try
            {
                _lock.EnterWriteLock();
                _searchIndex.AppendToIndex(name, fileItems);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            indexWatch.Stop();

            var traverseMs = traverseDirectoriesWatch.ElapsedMilliseconds;
            var indexMs = indexWatch.ElapsedMilliseconds;
            var totalMs = indexWatch.ElapsedMilliseconds + traverseDirectoriesWatch.ElapsedMilliseconds;

            Log.Information("Index " + name + " - " + fileItems.Count + " items. [ " + totalMs + " ms, tra: " + traverseMs + " ms, idx: " + indexMs + " ms ]");
        }
    }
}
