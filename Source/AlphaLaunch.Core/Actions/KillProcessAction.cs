using System;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public class KillProcessAction : IAction<RunningProcessInfo>
    {
        public void RunAction(RunningProcessInfo runningProcessInfo)
        {
            throw new Exception("BAM");
        }

        public string Name { get { return "Kill process"; } }
        public string BoostIdentifier { get { return "Kill process"; } }
    }

    public interface IAction<T> : IIndexable
    {
        void RunAction(T parameter);
    }

    public class RunningProcessInfo
    {
    }
}