using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App
{
    public class SearchItemModel
    {
        public string Name { get; set; }
        public float Score { get; set; }
        public string FullPath { get; set; }
        public Guid Id { get; set; }

        public SearchItemModel(string name, float score, string fullPath)
        {
            Name = name;
            Score = score;
            FullPath = fullPath;
        }

        public string DisplayName
        {
            get { return string.Format("{0} ({1})", Name, Score); }
        }
    }
}