using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class ContainingFolderConverter : IndexableBase, IConvert<FileItem, FolderItem>, IConvert<FolderItem, FolderItem>
{
    public FolderItem Convert(FileItem item)
    {
        return GetFolderItem(item.FullName);
    }

    public FolderItem Convert(FolderItem item)
    {
        return GetFolderItem(item.FullName);
    }

    private static FolderItem GetFolderItem(string fullName)
    {
        var directoryName = Path.GetDirectoryName(fullName);

        var directoryInfo = new DirectoryInfo(directoryName);

        if (!directoryInfo.Exists)
        {
            return null;
        }

        return new FolderItem(directoryName);
    }

    public override string Name => "Containing Folder";
}