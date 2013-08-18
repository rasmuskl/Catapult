using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AlphaLaunch.Core.Actions
{
    public class OpenAction : IItemSink<FileItem>
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
            };

            Process.Start(info);
        }
    }
}