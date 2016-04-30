using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class ContainingFolderAction : IndexableBase, IAction<FileItem>
    {
        public void RunAction(FileItem item)
        {
            var directoryName = Path.GetDirectoryName(item.FullName);

            var directoryInfo = new DirectoryInfo(directoryName);

            if (!directoryInfo.Exists)
            {
                return;
            }

            var info = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{directoryInfo.FullName}\"",
                UseShellExecute = true,
            };

            Process.Start(info)?.Dispose();
        }

        public override string Name => "Containing Folder";
    }
}