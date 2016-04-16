using System;
using System.Diagnostics;
using System.IO;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenLogFolderAction : IStandaloneAction
    {
        public void RunAction()
        {
            var logFolder = Path.Combine(Environment.CurrentDirectory, "Logs");

            if (!Directory.Exists(logFolder))
            {
                return;
            }
            
            Process.Start(logFolder);
        }

        public string Name => "Catapult: Open log folder";
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
}