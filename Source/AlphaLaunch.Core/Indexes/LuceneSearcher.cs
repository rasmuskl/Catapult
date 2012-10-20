using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AlphaLaunch.Core.Debug;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
{
    public class LuceneSearcher
    {
        private readonly RAMDirectory _directory;

        public LuceneSearcher()
        {
            _directory = new RAMDirectory();
        }

        public void IndexItems(FileItem[] items)
        {
            var indexAnalyzer = new LuceneIndexAnalyzer();

            using (var writer = new IndexWriter(_directory, indexAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var item in items)
                {
                    var document = new Document();

                    document.Add(new Field("id", item.Id.ToString(), Field.Store.YES, Field.Index.NO));
                    document.Add(new Field("name", item.Name, Field.Store.YES, Field.Index.ANALYZED));
                    document.Add(new Field("fullpath", Path.Combine(item.DirectoryName, item.Name), Field.Store.YES, Field.Index.NO));

                    writer.AddDocument(document);
                }

                writer.Optimize();
                writer.Commit();
                writer.Close();

                Log.Info("Index " + items.Length + " in Lucene.");
            }
        }

        public IEnumerable<SearchResult> Search(string search)
        {
            var stopwatch = Stopwatch.StartNew();
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            
            var queryParser = new QueryParser(Version.LUCENE_29, "name", analyzer);

            using (var indexSearcher = new IndexSearcher(_directory, true))
            {
                TopDocs topDocs = indexSearcher.Search(queryParser.Parse(search), 10);

                var searchResults = topDocs.ScoreDocs
                    .Select(x => new { Doc = indexSearcher.Doc(x.doc), ScoreDoc = x })
                    .Select(x => new SearchResult
                                     {
                                         Name = x.Doc.GetField("name").StringValue(), 
                                         FullPath = x.Doc.GetField("fullpath").StringValue(),
                                         Score =  x.ScoreDoc.score
                                     })
                    .ToArray();

                stopwatch.Stop();

                return searchResults;
            }
        }
    }
}