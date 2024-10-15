using Catapult.Core.Frecency;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public interface ISearchFrame
{
    SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage);
}