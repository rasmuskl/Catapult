using System;
using System.Diagnostics;
using System.IO;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
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

        public string Name => "AlphaLaunch: Open log folder";
        public string BoostIdentifier => "AlphaLaunchOpenLogFolder";

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