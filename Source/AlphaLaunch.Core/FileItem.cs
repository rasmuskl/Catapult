using System.IO;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core
{
    public class FileItem : IIndexable
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public string BoostIdentifier { get { return FullName; } }

        public FileItem(string fullName)
        {
            FullName = fullName;
            Name = Path.GetFileName(fullName);
        }

        public object GetDetails()
        {
            return new FileItemDetails(FullName);
        }
    }
}