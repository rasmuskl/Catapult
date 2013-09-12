using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Core.Indexes.Extensions
{
    public class ExtensionInfo
    {
        public string Extension { get; private set; }

        public ExtensionInfo(string extension)
        {
            Extension = extension;
        }
    }
}