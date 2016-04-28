﻿using System.Diagnostics;
using System.IO;
using Catapult.Core.Icons;

namespace Catapult.Core.Actions
{
    public class OpenAction : IAction<FileItem>
    {
        public void RunAction(FileItem item)
        {
            var fileInfo = new FileInfo(item.FullName);

            if (!fileInfo.Exists)
            {
                return;
            }

            var info = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{fileInfo.FullName}\"",
                UseShellExecute = true,
            };

            Process.Start(info)?.Dispose();
        }

        public string Name => "Open";
        public string BoostIdentifier => "Open";
        public object GetDetails()
        {
            return "Open";
        }

        public IIconResolver GetIconResolver()
        {
            return null;
        }
    }
}