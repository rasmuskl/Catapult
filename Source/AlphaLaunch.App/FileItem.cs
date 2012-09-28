using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App
{
    public class FileItem
    {
        private readonly string _fullName;

        public FileItem(string fullName)
        {
            _fullName = fullName;
        }
    }
}