using System.Reflection;
using Catapult.Core.Indexes;
using Microsoft.Win32;

namespace Catapult.Core.Actions
{
    public class EnableRunAtStartUpAction : IndexableBase, IStandaloneAction
    {
        public const string CatapultKey = "Catapult";

        public void Run()
        {
            EnableRunAtStartUp();
        }

        public static void EnableRunAtStartUp()
        {
            var location = Assembly.GetEntryAssembly().Location;
            RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            runKey?.SetValue(CatapultKey, $"\"{location}\"");
        }

        public static void UpdateRunAtStartUp()
        {
            var location = Assembly.GetEntryAssembly().Location;
            RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (runKey?.GetValue(CatapultKey) != null)
            {
                runKey.SetValue(CatapultKey, $"\"{location}\"");
            }
        }

        public override string Name => "Catapult: Enable run at Windows start";
    }
}