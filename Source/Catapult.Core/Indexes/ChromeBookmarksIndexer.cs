using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Catapult.Core.Indexes
{
    public class ChromeBookmarksIndexer
    {
        public BookmarkItem[] GetBookmarkItems()
        {
            try
            {
                string bookmarksFilePath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Google\Chrome\User Data\Default\Bookmarks");

                if (!File.Exists(bookmarksFilePath))
                {
                    return new BookmarkItem[0];
                }

                var bookmarkCollectionJson = File.ReadAllText(bookmarksFilePath);

                JToken rootToken = JToken.Parse(bookmarkCollectionJson);

                JToken rootsToken = rootToken["roots"];

                if (rootsToken == null)
                {
                    return new BookmarkItem[0];
                }

                var chromeBookmarks = new List<ChromeBookmark>();

                foreach (JToken token in rootsToken)
                {
                    var property = token as JProperty;

                    if (property?.Value is JObject)
                    {
                        chromeBookmarks.Add(property.Value.ToObject<ChromeBookmark>());
                    }
                }

                ChromeBookmark[] urlBookmarks = chromeBookmarks.SelectMany(x => x.Flatten()).Where(x => string.Equals(x.Type, "url", StringComparison.InvariantCultureIgnoreCase)).ToArray();

                return urlBookmarks.Select(x => new BookmarkItem(x.Name, x.Url, "Chrome bookmark")).ToArray();
            }
            catch (Exception ex)
            {
                Log.Error("Parsing Chome bookmarks failed.", ex);
            }

            return new BookmarkItem[0];
        }

        internal class ChromeBookmark
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Url { get; set; }
            public ChromeBookmark[] Children { get; set; }

            public IEnumerable<ChromeBookmark> Flatten()
            {
                yield return this;

                foreach (ChromeBookmark child in Children ?? new ChromeBookmark[0])
                {
                    foreach (ChromeBookmark bookmark in child.Flatten())
                    {
                        yield return bookmark;
                    }
                }
            }
        }
    }
}