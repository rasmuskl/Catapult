using Newtonsoft.Json.Linq;
using Serilog;

namespace Catapult.Core.Indexes;

public class EdgeBookmarksIndexer
{
    public BookmarkItem[] GetBookmarkItems()
    {
        var userDataPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Microsoft\Edge\User Data\");
        var bookmarkFilePaths = Directory.EnumerateFiles(userDataPath, "Bookmarks", SearchOption.AllDirectories);
        var bookmarkItems = new List<BookmarkItem>();

        foreach (var bookmarkFilePath in bookmarkFilePaths)
        {
            try
            {
                if (!File.Exists(bookmarkFilePath))
                {
                    continue;
                }

                var bookmarkCollectionJson = File.ReadAllText(bookmarkFilePath);

                JToken rootToken = JToken.Parse(bookmarkCollectionJson);

                JToken rootsToken = rootToken["roots"];

                if (rootsToken == null)
                {
                    continue;
                }

                var chromeBookmarks = new List<EdgeBookmark>();

                foreach (var token in rootsToken)
                {
                    var property = token as JProperty;

                    if (property?.Value is JObject)
                    {
                        chromeBookmarks.Add(property.Value.ToObject<EdgeBookmark>());
                    }
                }

                var urlBookmarks = chromeBookmarks.SelectMany(x => x.Flatten()).Where(x => string.Equals(x.Type, "url", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                bookmarkItems.AddRange(urlBookmarks.Select(x => new BookmarkItem(x.Name, x.Url, "Edge bookmark")));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Parsing Edge bookmarks failed");
            }
        }

        return bookmarkItems.ToArray();
    }

    internal class EdgeBookmark
    {
        public string Name { get; init; }
        public string Type { get; init; }
        public string Url { get; init; }
        public EdgeBookmark[] Children { get; init; } = [];

        public IEnumerable<EdgeBookmark> Flatten()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var bookmark in child.Flatten())
                {
                    yield return bookmark;
                }
            }
        }
    }
}