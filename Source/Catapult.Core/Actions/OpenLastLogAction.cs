using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenLastLogAction : IStandaloneAction
    {
        public void RunAction()
        {
            var logFolder = Path.Combine(Environment.CurrentDirectory, "Logs");

            if (!Directory.Exists(logFolder))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(logFolder);

            var lastLogFile = directoryInfo.GetFiles("*.log")
                .OrderByDescending(x => x.LastWriteTimeUtc)
                .FirstOrDefault();

            if (lastLogFile == null)
            {
                return;
            }

            Process.Start(lastLogFile.FullName);
        }

        public string Name => "Catapult: Open last log";
        public string BoostIdentifier => "CatapulthOpenLastLog";

        public object GetDetails()
        {
            return "Open last debug log";
        }

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}