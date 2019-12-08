using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Catapult.Core.Indexes.Extensions;
using Catapult.Core.Selecta;
using Newtonsoft.Json;
using Serilog;

namespace Catapult.Core.Indexes
{
    public class FileIndexStore
    {
        public static readonly FileIndexStore Instance = new FileIndexStore();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private FileIndexData _indexData = new FileIndexData();

        private FileIndexStore()
        {
            TryRestoreIndex();
        }

        public void IndexDirectory(string path, HashSet<string> ignoredDirectories, ExtensionContainer extensionContainer)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                var traverseDirectoriesWatch = Stopwatch.StartNew();

                var allFiles = SafeWalk.EnumerateFiles(path, ignoredDirectories);

                var paths = allFiles
                    .Where(x => extensionContainer.IsKnownExtension(Path.GetExtension(x)))
                    .Distinct()
                    .ToArray();

                traverseDirectoriesWatch.Stop();

                var indexWatch = Stopwatch.StartNew();

                try
                {
                    _lock.EnterWriteLock();
                    _indexData.Update(path, paths);
                    SaveIndex();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                indexWatch.Stop();

                var traverseMs = traverseDirectoriesWatch.ElapsedMilliseconds;
                var indexMs = indexWatch.ElapsedMilliseconds;
                var totalMs = indexWatch.ElapsedMilliseconds + traverseDirectoriesWatch.ElapsedMilliseconds;

                Log.Information($"Index {path} - {paths.Length} items. [ {totalMs} ms, tra: {traverseMs} ms, idx: {indexMs} ms ]");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to index path: {path}, message: {message}", path, e.Message);
            }
        }

        public bool IsIndexed(string path)
        {
            return _indexData.HasPath(path);
        }

        public string[] GetIndexedPaths(string path)
        {
            return _indexData.GetPaths(path) ?? new string[0];
        }

        private void TryRestoreIndex()
        {
            try
            {
                if (!File.Exists(CatapultPaths.IndexPath))
                {
                    return;
                }

                var indexJson = File.ReadAllText(CatapultPaths.IndexPath);
                _indexData = JsonConvert.DeserializeObject<FileIndexData>(indexJson) ?? new FileIndexData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Index restore failed.");
                _indexData = new FileIndexData();
            }
        }

        private void SaveIndex()
        {
            if (!Directory.Exists(CatapultPaths.DataPath))
            {
                Directory.CreateDirectory(CatapultPaths.DataPath);
            }

            var indexJson = JsonConvert.SerializeObject(_indexData);
            File.WriteAllText(CatapultPaths.IndexPath, indexJson);
        }
    }

    public class FileIndexData
    {
        public Dictionary<string, PathIndexData> Data { get; set; } = new Dictionary<string, PathIndexData>();

        public void Update(string path, string[] filePaths)
        {
            PathIndexData data;

            if (!Data.TryGetValue(path, out data))
            {
                data = new PathIndexData();
                Data[path] = data;
            }

            data.FilePaths = filePaths;
            data.LastIndexedUtc = DateTime.UtcNow;
        }

        public bool HasPath(string path)
        {
            return Data.ContainsKey(path);
        }

        public string[] GetPaths(string path)
        {
            PathIndexData data;

            if (!Data.TryGetValue(path, out data))
            {
                return null;
            }

            return data.FilePaths;
        }
    }

    public class PathIndexData
    {
        public string[] FilePaths { get; set; }
        public DateTime LastIndexedUtc { get; set; }
    }
}
