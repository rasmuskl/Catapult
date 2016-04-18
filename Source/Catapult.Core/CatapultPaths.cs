using System;
using System.IO;

namespace Catapult.Core
{
    public static class CatapultPaths
    {
        public static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Catapult", "config.json");
        public static string FrecencyPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Catapult", "frecency.json");
    }
}