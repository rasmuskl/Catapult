using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core
{
    public class FileItem : IIndexable
    {
        public string FullName { get; set; }
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public string BoostIdentifier { get { return FullName; } }

        public FileItem(string fullName)
        {
            FullName = fullName;
            DirectoryName = Path.GetDirectoryName(fullName);
            Name = Path.GetFileName(fullName);
        }
    }
}