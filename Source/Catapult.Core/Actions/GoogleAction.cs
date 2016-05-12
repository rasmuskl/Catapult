using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Catapult.Core.Indexes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Catapult.Core.Actions
{
    public class GoogleAction : IndexableBase, IStandaloneAction, IAction<StringIndexable>, IAutocomplete
    {
        public void Run()
        {
            Process.Start("https://www.google.com/");
        }

        public void Run(StringIndexable stringIndexable)
        {
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public SearchResult[] GetAutocompleteResults(string search)
        {
            if (search.IsNullOrWhiteSpace())
            {
                return new SearchResult[0];
            }

            var searchResults = new List<SearchResult>();

            using (var webClient = new WebClient())
            {
                string suggestionJson = webClient.DownloadString("http://suggestqueries.google.com/complete/search?client=firefox&q=" + Uri.EscapeDataString(search));
                JArray suggestions = (JArray)JsonConvert.DeserializeObject<object[]>(suggestionJson)[1];

                foreach (string suggestion in suggestions.Children<JToken>().Select(x => x.ToString()).Except(new[] { search }).Distinct())
                {
                    searchResults.Add(new SearchResult(suggestion, 0, new StringIndexable(suggestion), ImmutableHashSet.Create<int>()));
                }
            }

            return searchResults.ToArray();
        }

        public override string Name => "Google search";
    }
}