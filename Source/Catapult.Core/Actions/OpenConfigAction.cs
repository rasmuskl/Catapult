using System.Diagnostics;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class OpenConfigAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        if (!File.Exists(CatapultPaths.ConfigPath))
        {
            return;
        }

        Process.Start(CatapultPaths.ConfigPath);
    }

    public override string Name => "Catapult: Open config file";
}