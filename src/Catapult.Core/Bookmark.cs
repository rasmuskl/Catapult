using Catapult.Core.Indexes;

namespace Catapult.Core;

public class BookmarkItem : IndexableBase
{
    public BookmarkItem(string name, string url, string? details = null)
    {
        Name = name;
        Url = url;
        Details = details;
    }

    public string Url { get; set; }

    public override string Name { get; }
    public override string? Details { get; }

}