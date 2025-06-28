using System.Diagnostics;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class OpenLastLogAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        if (!Directory.Exists(CatapultPaths.LogPath))
        {
            return;
        }

        var directoryInfo = new DirectoryInfo(CatapultPaths.LogPath);

        var lastLogFile = directoryInfo.GetFiles("*.log")
            .OrderByDescending(x => x.LastWriteTimeUtc)
            .FirstOrDefault();

        if (lastLogFile == null)
        {
            return;
        }

        var processStartInfo = new ProcessStartInfo(lastLogFile.FullName)
        {
            UseShellExecute = true
        };

        Process.Start(processStartInfo);
    }

    public override string Name => "Catapult: Open last log";
}