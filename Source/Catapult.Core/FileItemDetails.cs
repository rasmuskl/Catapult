using System.Drawing;

namespace Catapult.Core
{
    public class FileItemDetails
    {
        public Icon Icon { get; }
        public string FullName { get; set; }
        public string ResolvePath { get; set; }

        public FileItemDetails(string fullName)
        {
            FullName = fullName;
            ResolvePath = LnkResolver.ResolveShortcut(fullName);
            Icon = Icon.ExtractAssociatedIcon(fullName);
        }
    }
}