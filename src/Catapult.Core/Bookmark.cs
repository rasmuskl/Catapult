using Catapult.Core.Indexes;

namespace Catapult.Core;

public class BookmarkItem(string name, string url, string? details = null) : IndexableBase
{
    public string Url { get; set; } = url;

    public override string Name { get; } = name;
    public override string? Details { get; } = details;
}