using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App
{
    public class SearchItemModel
    {
        public string Name { get; set; }

        public SearchItemModel(string name)
        {
            Name = name;
        }
    }
}