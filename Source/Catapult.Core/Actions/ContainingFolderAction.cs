using System.Diagnostics;
using System.IO;
using Catapult.Core.Icons;

namespace Catapult.Core.Actions
{
    public class ContainingFolderAction : IAction<FileItem>
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

        public string Name => "Containing Folder";
        public string BoostIdentifier => "Containing Folder";

        public object GetDetails()
        {
            return "Containing folder";
        }

        public IIconResolver GetIconResolver()
        {
            return null;
        }
    }
}