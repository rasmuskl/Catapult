using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions;

public class IndexableSearchFrame(IIndexable[] indexables) : ISearchFrame
{
    private Searcher _selectaSeacher = Searcher.Create(indexables);
    public IIndexable[] Indexables { get; } = indexables;

    public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
    {
        var frecencyData = frecencyStorage.GetFrecencyData();
        Func<IIndexable, int> boosterFunc = x => frecencyData.GetValueOrDefault(x.BoostIdentifier, 0);
        _selectaSeacher = _selectaSeacher.Search(search, boosterFunc);
        return _selectaSeacher.SearchResults.Take(100).ToArray();
    }
}