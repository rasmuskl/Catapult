using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions;

public class IndexableSearchFrame : ISearchFrame
{
    private Searcher _selectaSeacher;
    public IIndexable[] Indexables { get; }

    public IndexableSearchFrame(IIndexable[] indexables)
    {
        Indexables = indexables;
        _selectaSeacher = Searcher.Create(indexables);
    }

    public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
    {
        var frecencyData = frecencyStorage.GetFrecencyData();
        Func<IIndexable, int> boosterFunc = x => frecencyData.ContainsKey(x.BoostIdentifier) ? frecencyData[x.BoostIdentifier] : 0;
        _selectaSeacher = _selectaSeacher.Search(search, boosterFunc);
        return _selectaSeacher.SearchResults.Take(100).ToArray();
    }
}