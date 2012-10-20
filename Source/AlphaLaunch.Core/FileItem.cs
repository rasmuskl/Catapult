using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core
{
    public class FileItem
    {
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public Guid Id { get; set; }

        public FileItem(string directoryName, string name, string extension)
        {
            DirectoryName = directoryName;
            Name = name;
            Extension = extension;
            Id = Guid.NewGuid();
        }
    }
}