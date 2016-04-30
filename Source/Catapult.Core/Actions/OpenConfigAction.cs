using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenConfigAction : IndexableBase, IStandaloneAction
    {
        public void RunAction()
        {
            if (!File.Exists(CatapultPaths.ConfigPath))
            {
                return;
            }

            Process.Start(CatapultPaths.ConfigPath);
        }

        public override string Name => "Catapult: Open config file";
    }
}