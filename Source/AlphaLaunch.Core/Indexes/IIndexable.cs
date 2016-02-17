using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
{
    public interface IIndexable
    {
        string Name { get; }
        string BoostIdentifier { get; }
        object GetDetails();
        Icon GetIcon();
    }
}