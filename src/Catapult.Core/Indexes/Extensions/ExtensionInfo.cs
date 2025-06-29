namespace Catapult.Core.Indexes.Extensions;

public class ExtensionInfo(string extension)
{
    public string Extension { get; private set; } = extension;
}