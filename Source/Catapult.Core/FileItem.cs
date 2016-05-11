using System;
using System.Collections.Generic;
using System.IO;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core
{
    public class FileItem : IIndexable
    {
        private static readonly HashSet<string> HiddenExtensions = new HashSet<string>(new[] { ".lnk", ".url" }, StringComparer.InvariantCultureIgnoreCase);

        public string FullName { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BoostIdentifier => FullName;

        public FileItem(string fullName)
        {
            FullName = fullName;

            if (HiddenExtensions.Contains(Path.GetExtension(FullName)))
            {
                Name = Path.GetFileNameWithoutExtension(FullName);
            }
            else
            {
                Name = Path.GetFileName(FullName);
            }

            Details = Path.GetDirectoryName(FullName);
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