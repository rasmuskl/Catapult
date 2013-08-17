using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AlphaLaunch.Core.Executors
{
    public class FileItemExecutor
    {
        public void Execute(object obj)
        {
            var fileItem = obj as FileItem;

            if (fileItem == null)
            {
                return;
            }

            var fileInfo = new FileInfo(fileItem.FullName);

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