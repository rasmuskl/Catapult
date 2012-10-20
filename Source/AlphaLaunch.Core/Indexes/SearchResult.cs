using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.Core.Indexes
{
    public class SearchResult
    {
        public string Name { get; set; }
        public float Score { get; set; }
        public string FullPath { get; set; }
    }
}