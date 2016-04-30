using System.Diagnostics;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class KillProcessAction : IndexableBase, IAction<RunningProcessInfo>
    {
        public void RunAction(RunningProcessInfo runningProcessInfo)
        {
            var process = Process.GetProcessById(runningProcessInfo.ProcessId);
            process.Kill();
        }

        public override string Name => "Kill process";
    }
}