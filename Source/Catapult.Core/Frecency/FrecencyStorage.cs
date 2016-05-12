using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Catapult.Core.Frecency
{
    public class FrecencyStorage
    {
        private readonly string _path;
        private readonly FrecencyData _data;

        public FrecencyStorage(string path)
        {
            _path = path;
            _data = RestoreData();
        }

        public void AddUse(string boostIdentifier, string searchString, int selectedIndex)
        {
            if (boostIdentifier.IsNullOrWhiteSpace())
            {
                return;
            }

            _data.AddUse(boostIdentifier, searchString, selectedIndex);
            SaveData();
        }

        private void SaveData()
        {
            var directoryName = Path.GetDirectoryName(_path);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(_path, JsonConvert.SerializeObject(_data, Formatting.Indented));
        }

        public Dictionary<string, int> GetFrecencyData()
        {
            var enumerable = (from entry in _data.Entries
                              group entry by entry.BoostIdentifier into x
                              let score = GetScore(x.Count(), x.Max(y => y.UtcUse))
                              select new { x.Key, Score = score });

            return enumerable.ToDictionary(x => x.Key, x => x.Score);
        }

        private int GetScore(int count, DateTime utcLatest)
        {
            var now = DateTime.UtcNow;

            if (utcLatest > now.AddDays(-1))
            {
                return 4 * count;
            }

            if (utcLatest > now.AddDays(-7))
            {
                return 2 * count;
            }

            return count;
        }

        private FrecencyData RestoreData()
        {
            if (!File.Exists(_path))
            {
                return new FrecencyData();
            }

            return JsonConvert.DeserializeObject<FrecencyData>(File.ReadAllText(_path));
        }
    }
}