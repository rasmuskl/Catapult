using System.Diagnostics;
using Catapult.Core.Indexes.Extensions;
using Catapult.Core.Selecta;
using Newtonsoft.Json;
using Serilog;

namespace Catapult.Core.Indexes;

public class FileIndexStore
{
    public static readonly FileIndexStore Instance = new();

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private FileIndexData _indexData = new();

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
        return _indexData.GetPaths(path) ?? [];
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
    public Dictionary<string, PathIndexData> Data { get; set; } = new();

    public void Update(string path, string[] filePaths)
    {
        if (!Data.TryGetValue(path, out var data))
        {
            data = new PathIndexData
            {
                FilePaths = filePaths
            };
            Data[path] = data;
        }

        data.FilePaths = filePaths;
        data.LastIndexedUtc = DateTime.UtcNow;
    }

    public bool HasPath(string path)
    {
        return Data.ContainsKey(path);
    }

    public string[]? GetPaths(string path)
    {
        if (!Data.TryGetValue(path, out var data))
        {
            return null;
        }

        return data.FilePaths;
    }
}

public class PathIndexData
{
    public required string[] FilePaths { get; set; }
    public DateTime LastIndexedUtc { get; set; }
}