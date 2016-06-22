using System;
using System.IO;

namespace Catapult.Core
{
    public static class CatapultPaths
    {
        public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Catapult");
        public static string ConfigPath => Path.Combine(DataPath, "config.json");
        public static string IndexPath => Path.Combine(DataPath, "index.json");
        public static string ClipboardPath => Path.Combine(DataPath, "clipboard.json");
        public static string FrecencyPath => Path.Combine(DataPath, "frecency.json");
        public static string LogPath => Path.Combine(DataPath, "logs");
        public static string NewVersionPath => Path.Combine(DataPath, "new_version");
    }
}