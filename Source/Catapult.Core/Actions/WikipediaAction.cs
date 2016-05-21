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
    public class WikipediaAction : IStandaloneAction, IAction<StringIndexable>, IAutocomplete
    {
        public void Run()
        {
            Process.Start("https://wikipedia.org/");
        }

        public void Run(StringIndexable stringIndexable)
        {
            Process.Start("https://wikipedia.org/wiki/Special:Search?search=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public string Name => "Wikipedia search";
        public string Details => null;
        
        public string BoostIdentifier => Name;

        public object GetDetails()
        {
            return Name;
        }

        public IIconResolver GetIconResolver()
        {
            return new FaviconIconResolver("http://www.wikipedia.com/favicon.ico");
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
                string suggestionXml = webClient.DownloadString("https://en.wikipedia.org/w/api.php?action=query&format=xml&generator=prefixsearch&prop=pageprops%7Cpageimages%7Cpageterms&ppprop=displaytitle&piprop=thumbnail&pithumbsize=32&pilimit=10&wbptterms=description&gpslimit=10&gpssearch=" + Uri.EscapeDataString(search));
                var document = XDocument.Parse(suggestionXml);

                var pageElements = document.XPathSelectElements("//page");

                foreach (var pageElement in pageElements)
                {
                    var title = pageElement.Attribute("title").Value;

                    var termElement = pageElement.XPathSelectElement(".//term");
                    var description = termElement?.Value;

                    var thumbnailElement = pageElement.XPathSelectElement(".//thumbnail");
                    var thumbnailUrl = thumbnailElement?.Attribute("source")?.Value;

                    searchResults.Add(new SearchResult(title, 0, new StringIndexable(title, description, new UrlIconResolver(thumbnailUrl)), ImmutableHashSet.Create<int>()));
                }
            }

            return searchResults.ToArray();
        }
    }
}