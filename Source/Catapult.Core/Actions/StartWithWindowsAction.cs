using System;
using System.IO;
using Catapult.Core.Indexes;
using Microsoft.Win32;

namespace Catapult.Core.Actions
{
    public class EnableRunAtStartUpAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Catapult", "Catapult.appref-ms");
            RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            runKey.SetValue("Catapult", $"\"{path}\"");
        }

        public override string Name => "Catapult: Enable run at Windows start";
    }
}