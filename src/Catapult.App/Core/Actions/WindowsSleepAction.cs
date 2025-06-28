using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class WindowsSleepAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "rundll32.exe"), "powrprof.dll,SetSuspendState 0,1,0");
    }

    public override string Name => "Windows: Sleep";
}