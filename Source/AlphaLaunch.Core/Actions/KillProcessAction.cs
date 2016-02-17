using System;
using System.Diagnostics;
using System.Drawing;

namespace AlphaLaunch.Core.Actions
{
    public class KillProcessAction : IAction<RunningProcessInfo>
    {
        public void RunAction(RunningProcessInfo runningProcessInfo)
        {
            var process = Process.GetProcessById(runningProcessInfo.ProcessId);
            process.Kill();
        }

        public string Name { get { return "Kill process"; } }
        public string BoostIdentifier { get { return "Kill process"; } }

        public object GetDetails()
        {
            return "Kill process";
        }

        public Icon GetIcon()
        {
            return null;
        }
    }
}