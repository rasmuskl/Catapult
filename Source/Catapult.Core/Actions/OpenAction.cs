using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenAction : IndexableBase, IAction<FileItem>
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

        public override string Name => "Open";
    }
}