using System;
using System.Linq;
using System.Collections.Generic;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Experiments
{
    public static class TestExtensions
    {
        public static IEnumerable<IIndexable> ToStringIndexables(this IEnumerable<string> strings)
        {
            return strings.Select(x => new StringIndexable(x));
        }
    }
}