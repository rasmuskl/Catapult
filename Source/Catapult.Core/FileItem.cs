using System.IO;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core
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

        public IIconResolver GetIconResolver()
        {
            return new FileIconResolver(FullName);
        }
    }
}