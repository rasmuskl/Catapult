using System.Diagnostics;
using System.IO;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenConfigAction : IStandaloneAction
    {
        public void RunAction()
        {
            if (!File.Exists(CatapultPaths.ConfigPath))
            {
                return;
            }

            Process.Start(CatapultPaths.ConfigPath);
        }

        public string Name => "Catapult: Open config file";
        public string BoostIdentifier => "CatapulthOpenConfigFile";

        public object GetDetails()
        {
            return "Open config file";
        }

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}