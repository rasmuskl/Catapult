using System;
using System.Collections.Generic;

namespace AlphaLaunch.Core.Frecency
{
    public class FrecencyData
    {
        public List<FrecencyEntry> Entries { get; private set; } = new List<FrecencyEntry>();
     
        public void AddUse(string boostIdentifier, string searchString)
        {
            Entries.Add(new FrecencyEntry(boostIdentifier, searchString, DateTime.UtcNow));
        }
    }
}