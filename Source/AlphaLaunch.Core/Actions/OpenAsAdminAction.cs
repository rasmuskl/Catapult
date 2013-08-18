using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AlphaLaunch.Core.Actions
{
    public class OpenAsAdminAction : IItemSink<FileItem>
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
                FileName = fileInfo.FullName,
                Verb = "runas",
            };

            Process.Start(info);
        }
    }
}