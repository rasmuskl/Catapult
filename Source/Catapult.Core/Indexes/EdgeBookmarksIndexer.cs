using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Catapult.Core.Indexes
{
    public class EdgeBookmarksIndexer
    {
        public BookmarkItem[] GetBookmarkItems()
        {
            try
            {
                string bookmarksFilePath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Microsoft\Edge\User Data\Default\Bookmarks");

                if (!File.Exists(bookmarksFilePath))
                {
                    return Array.Empty<BookmarkItem>();
                }

                var bookmarkCollectionJson = File.ReadAllText(bookmarksFilePath);

                JToken rootToken = JToken.Parse(bookmarkCollectionJson);

                JToken rootsToken = rootToken["roots"];

                if (rootsToken == null)
                {
                    return Array.Empty<BookmarkItem>();
                }

                var chromeBookmarks = new List<EdgeBookmark>();

                foreach (JToken token in rootsToken)
                {
                    var property = token as JProperty;

                    if (property?.Value is JObject)
                    {
                        chromeBookmarks.Add(property.Value.ToObject<EdgeBookmark>());
                    }
                }

                var urlBookmarks = chromeBookmarks.SelectMany(x => x.Flatten()).Where(x => string.Equals(x.Type, "url", StringComparison.InvariantCultureIgnoreCase)).ToArray();

                return urlBookmarks.Select(x => new BookmarkItem(x.Name, x.Url, "Edge bookmark")).ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Parsing Edge bookmarks failed.");
            }

            return Array.Empty<BookmarkItem>();
        }

        internal class EdgeBookmark
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public EdgeBookmark[] Children { get; set; }

            public IEnumerable<EdgeBookmark> Flatten()
            {
                yield return this;

                foreach (EdgeBookmark child in Children ?? Array.Empty<EdgeBookmark>())
                {
                    foreach (EdgeBookmark bookmark in child.Flatten())
                    {
                        yield return bookmark;
                    }
                }
            }
        }
    }
}