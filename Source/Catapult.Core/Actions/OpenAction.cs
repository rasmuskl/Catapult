using System.Diagnostics;
using System.IO;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class OpenAction : IndexableBase, IAction<FileItem>, IAction<FolderItem>, IAction<BookmarkItem>
    {
        public void Run(FileItem item)
        {
            var fileInfo = new FileInfo(item.FullName);

            if (!fileInfo.Exists)
            {
                return;
            }

            Launch(fileInfo.FullName);
        }

        public void Run(FolderItem item)
        {
            var directoryInfo = new DirectoryInfo(item.FullName);

            if (!directoryInfo.Exists)
            {
                return;
            }

            Launch(directoryInfo.FullName);
        }

        public void Run(BookmarkItem item)
        {
            Launch(item.Url);
        }

        private static void Launch(string fullName)
        {
            var info = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{fullName}\"",
                UseShellExecute = true,
            };

            Process.Start(info)?.Dispose();
        }

        public override string Name => "Open";
        public override string BoostIdentifier => string.Empty; // To force not appearing in top of frecency
    }
}