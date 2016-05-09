using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public interface IAutocomplete
    {
        SearchResult[] GetAutocompleteResults(string search);
    }
}