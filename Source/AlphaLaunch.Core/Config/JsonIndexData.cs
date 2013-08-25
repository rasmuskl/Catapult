using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Config
{
    public class JsonIndexData
    {
        public JsonIndexData()
        {
            BoostEntries = new Dictionary<string, EntryBoost>();
        }

        public Dictionary<string, EntryBoost> BoostEntries { get; set; } 
    }
}