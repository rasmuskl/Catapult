using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Experiments
{
    public class StringIndexable : IIndexable
    {
        public StringIndexable(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public string BoostIdentifier { get { return Name; } }

        public object GetDetails()
        {
            return Name;
        }

        public Icon GetIcon()
        {
            return null;
        }
    }
}