using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App
{
    public class FileItem
    {
        public string DirectoryName { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }

        public FileItem(string directoryName, string name, string extension)
        {
            DirectoryName = directoryName;
            Name = name;
            Extension = extension;
        }
    }
}