using System;
using System.Diagnostics;
using AlphaLaunch.Core.Indexes;

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
    }

    public interface IAction<T> : IIndexable
    {
        void RunAction(T parameter);
    }

    public class RunningProcessInfo : IIndexable
    {
        private readonly string _processName;
        private readonly string _title;
        private readonly int _processId;

        public RunningProcessInfo(string processName, string title, int processId)
        {
            _processName = processName;
            _title = title;
            _processId = processId;
        }

        public int ProcessId
        {
            get { return _processId; }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_title))
                {
                    return string.Format("{0} [{1}]", _processName, _processId);
                }

                return string.Format("{0} - {1} [{2}]", _processName, _title, _processId);
            }
        }

        public string BoostIdentifier { get { return Name; } }
    }
}