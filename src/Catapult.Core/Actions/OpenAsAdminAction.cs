using System.Diagnostics;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class OpenAsAdminAction : IndexableBase, IAction<FileItem>
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
        
    private static void Launch(string fullName)
    {
        var info = new ProcessStartInfo
        {
            FileName = $"{fullName}",
            UseShellExecute = true,
            Verb = "runas",
        };

        Process.Start(info)?.Dispose();
    }

    public override string Name => "Open as Administrator";
    public override string BoostIdentifier => string.Empty; // To force not appearing in top of frecency
}