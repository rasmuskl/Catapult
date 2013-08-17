using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Experiments
{
    public class Result
    {
        public string MatchedString { get; private set; }
        public double Score { get; private set; }
        public ImmutableDictionary<int, double> MatchedIndexes { get; private set; }

        public Result(string matchedString, double score, ImmutableDictionary<int, double> matchedIndexes)
        {
            MatchedString = matchedString;
            Score = score;
            MatchedIndexes = matchedIndexes;
        }

        public override string ToString()
        {
            return string.Format("MatchedString: {0}, Score: {1}", MatchedString, Score);
        }
    }
}