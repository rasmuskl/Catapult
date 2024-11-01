using System.Diagnostics;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class OpenLogFolderAction : IStandaloneAction
{
    public void Run()
    {
        if (!Directory.Exists(CatapultPaths.LogPath))
        {
            return;
        }

        var processStartInfo = new ProcessStartInfo(CatapultPaths.LogPath)
        {
            UseShellExecute = true
        };

        Process.Start(processStartInfo);
    }

    public string Name => "Catapult: Open log folder";
    public string Details => null;

    public string BoostIdentifier => "CatapultOpenLogFolder";

    public object GetDetails()
    {
        return "Open debug log folder";
    }

    public IIconResolver GetIconResolver()
    {
        return new EmptyIconResolver();
    }
}