using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using AlphaLaunch.Experiments;

namespace AlphaLaunch.Core
{
    public class FileItem : IIndexable
    {
        public string FullName { get; set; }
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }

        public FileItem(string fullName)
        {
            FullName = fullName;
            DirectoryName = Path.GetDirectoryName(fullName);
            Name = Path.GetFileName(fullName);
            Id = Guid.NewGuid();
        }
    }
}