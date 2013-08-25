using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
{
    public class EntryBoost
    {
        public string BoostIdentifier { get; private set; }

        public EntryBoost(string boostIdentifier)
        {
            BoostIdentifier = boostIdentifier;
        }
    }
}