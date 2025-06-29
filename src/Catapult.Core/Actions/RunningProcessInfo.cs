using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class RunningProcessInfo(string processName, string title, int processId) : IndexableBase
{
    public int ProcessId => processId;

    public override string Name
    {
        get
        {
            if (title.IsNullOrWhiteSpace())
            {
                return $"{processName} [{processId}]";
            }

            return $"{processName} - {title} [{processId}]";
        }
    }
}