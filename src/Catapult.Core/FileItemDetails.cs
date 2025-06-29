namespace Catapult.Core;

public class FileItemDetails(string fullName)
{
    public string FullName { get; set; } = fullName;
    public string ResolvePath { get; set; } = LnkResolver.ResolveShortcut(fullName);
}