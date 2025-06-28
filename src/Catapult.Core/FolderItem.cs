using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core;

public class FolderItem : IIndexable
{
    public string FullName { get; set; }
    public string Name { get; set; }
    public string? Details { get; set; }
    public string BoostIdentifier => FullName;

    public FolderItem(string fullName)
    {
        FullName = fullName;
        Name = Path.GetFileName(FullName);
        Details = Path.GetDirectoryName(fullName);
    }

    public object GetDetails()
    {
        return FullName;
    }

    public IIconResolver GetIconResolver()
    {
        return new FileIconResolver(FullName);
    }
}