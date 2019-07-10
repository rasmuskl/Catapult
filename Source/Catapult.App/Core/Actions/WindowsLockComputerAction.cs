using System;
using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class WindowsLockComputerAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "rundll32.exe"), "User32.dll,LockWorkStation");
        }

        public override string Name => "Windows: Lock computer";
    }
}