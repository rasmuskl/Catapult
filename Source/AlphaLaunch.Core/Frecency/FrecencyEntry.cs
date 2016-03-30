using System;

namespace AlphaLaunch.Core.Frecency
{
    public class FrecencyEntry
    {
        public string BoostIdentifier { get; set; }
        public string SearchString { get; set; }
        public DateTime UtcUse { get; set; }

        private FrecencyEntry()
        {
        }

        public FrecencyEntry(string boostIdentifier, string searchString, DateTime utcUse)
        {
            BoostIdentifier = boostIdentifier;
            SearchString = searchString;
            UtcUse = utcUse;
        }
    }
}