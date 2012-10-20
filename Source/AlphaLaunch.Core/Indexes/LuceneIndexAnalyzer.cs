using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;

namespace AlphaLaunch.Core.Indexes
{
    public class LuceneIndexAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new WhitespaceTokenizer(reader);
            var edgeNGram = new EdgeNGramTokenFilter(tokenizer, "front", 1, 25);
            var lowerCase = new LowerCaseFilter(edgeNGram);
            return lowerCase;
        }
    }
}