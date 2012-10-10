using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using AlphaLaunch.App.Config;
using AlphaLaunch.App.Debug;

namespace AlphaLaunch.App.Indexes
{
    public class IndexStore
    {
        public static readonly IndexStore Instance = new IndexStore();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly List<FileItem> _fileItems = new List<FileItem>();

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

        private void IndexDirectory(string s, string path)
        {
            var stopwatch = Stopwatch.StartNew();

            var dropbox = new DirectoryInfo(path);
            var fileItems = GetFiles(dropbox).ToArray();

            try
            {
                _lock.EnterWriteLock();
                _fileItems.AddRange(fileItems);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            stopwatch.Stop();

            Log.Info("Indexed " + s + " - " + fileItems.Length + " items. [" + stopwatch.ElapsedMilliseconds + " ms]");
        }

        private IEnumerable<FileItem> GetFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Select(x => new FileItem(x.DirectoryName, x.Name, x.Extension))
                .Concat(directory.GetDirectories().SelectMany(GetFiles));
        }

        public IEnumerable<FileItem> Search(string search)
        {
            return _fileItems
                .Where(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1)
                .OrderBy(x => x.Name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase))
                .Take(10);
        }

        public FileItem GetById(Guid id)
        {
            return _fileItems.FirstOrDefault(x => x.Id == id);
        }
    }
}