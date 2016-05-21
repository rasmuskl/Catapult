using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class PathOfExileWikiAction : IStandaloneAction, IAction<StringIndexable>, IAutocomplete
    {
        public void Run()
        {
            Process.Start("http://pathofexile.gamepedia.com/");
        }

        public void Run(StringIndexable stringIndexable)
        {
            Process.Start("http://pathofexile.gamepedia.com/index.php?search=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public string Name => "Path of Exile wiki search";
        public string Details => null;

        public string BoostIdentifier => Name;

        public object GetDetails()
        {
            return Name;
        }

        public IIconResolver GetIconResolver()
        {
            return new FaviconIconResolver("https://hydra-media.cursecdn.com/pathofexile.gamepedia.com/6/64/Favicon.ico");
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
                string suggestionXml = webClient.DownloadString("http://pathofexile.gamepedia.com/api.php?action=opensearch&format=xml&search=" + Uri.EscapeDataString(search));
                var document = XDocument.Parse(suggestionXml);

                var namespaceResolver = new XmlNamespaceManager(new NameTable());
                namespaceResolver.AddNamespace("x", "http://opensearch.org/searchsuggest2");
                var itemTextElements = document.XPathSelectElements("//x:Item/x:Text", namespaceResolver);

                foreach (string suggestion in itemTextElements.Select(x => x.Value).Except(new[] { search }).Distinct())
                {
                    searchResults.Add(new SearchResult(suggestion, 0, new StringIndexable(suggestion), ImmutableHashSet.Create<int>()));
                }
            }

            return searchResults.ToArray();
        }
    }
}