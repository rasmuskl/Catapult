﻿using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class WindowsRestartAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shutdown.exe"), "-r -t 00");
    }

    public override string Name => "Windows: Restart / Reboot";
}