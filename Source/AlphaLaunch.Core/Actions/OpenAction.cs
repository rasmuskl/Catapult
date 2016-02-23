using System.Diagnostics;
using System.IO;

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
                FileName = "explorer",
                Arguments = $"\"{fileInfo.FullName}\"",
                UseShellExecute = true,
            };

            Process.Start(info)?.Dispose();
        }
    }
}