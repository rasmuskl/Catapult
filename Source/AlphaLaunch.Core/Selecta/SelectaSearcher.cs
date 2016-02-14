using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AlphaLaunch.Core.Debug;
using AlphaLaunch.Core.Indexes;
using AlphaLaunch.Core.Indexes.Extensions;

namespace AlphaLaunch.Core.Selecta
{
    public class Searcher
    {
        private readonly string[] _allItems;
        private readonly string[] _matchedItems;
        private readonly SelectaSearcher _selecta;

        public string SearchString { get; }
        public SearchResult[] SearchResults { get; }

        public static Searcher Create(string[] allItems)
        {
            return new Searcher(allItems, allItems, string.Empty, new SearchResult[0]);
        }

        private Searcher(string[] allItems, string[] matchedItems, string searchString, SearchResult[] searchResults)
        {
            _allItems = allItems;
            _matchedItems = matchedItems;
            SearchString = searchString;
            SearchResults = searchResults;
            _selecta = new SelectaSearcher();
        }

        public Searcher Search(string searchString)
        {
            searchString = searchString ?? string.Empty;

            var items = _matchedItems;

            if (!searchString.StartsWith(SearchString))
            {
                items = _allItems;
            }

            var scoreStopwatch = Stopwatch.StartNew();

            var matches = items
                .Select(x => new { MatchScore = _selecta.Score(searchString, Path.GetFileName(x)), Name = Path.GetFileName(x), FullName = x })
                .Where(x => x.MatchScore.Score != int.MaxValue)
                .ToArray();

            var searchResults = matches
                .OrderBy(x => x.MatchScore.Score)
                .ThenBy(x => x.Name.Length)
                .ThenBy(x => x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex)
                .Select(x => new SearchResult(x.Name, x.MatchScore.Score, new FileItem(x.FullName), ImmutableDictionary.Create(Enumerable.Range(x.MatchScore.Range.StartIndex, 1 + x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex).Select(i => new KeyValuePair<int, double>(i, 0.0)))))
                .Take(50)
                .ToArray();

            var matchedItems = matches.Select(x => x.FullName).ToArray();

            scoreStopwatch.Stop();

            Log.Info($"Found {matches.Length} results of {items.Length} [ scr: {scoreStopwatch.ElapsedMilliseconds} ms ]");

            return new Searcher(_allItems, matchedItems, searchString, searchResults);
        }
    }

    public static class SearchResources
    {
        public static string[] GetFiles()
        {
            var stopwatch = Stopwatch.StartNew();

            var paths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                //Environment.GetFolderPath(Environment.SpecialFolder.Recent),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                @"c:\dev",
            };


            var ignoredDirectories = new HashSet<string>(new[] { "node_modules", ".git", "scratch" });

            var extensionContainer = new ExtensionContainer(new[] { new ExtensionInfo(".lnk"), new ExtensionInfo(".exe"), new ExtensionInfo(".sln"), new ExtensionInfo(".url"),  });

            var allFiles = paths.SelectMany(x => SafeWalk.EnumerateFiles(x, ignoredDirectories));
            var files = allFiles.Where(x => extensionContainer.IsKnownExtension(Path.GetExtension(x))).ToArray();

            stopwatch.Stop();

            Log.Info($"Traversed {files.Length} files in {stopwatch.ElapsedMilliseconds} ms");

            return files;
        }
    }

    public class SelectaSearcher
    {
        public SearchResult[] Search(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return new SearchResult[0];
            }

            var fileStopwatch = Stopwatch.StartNew();

            var files = SearchResources.GetFiles().Select(x => new FileItem(x)).ToArray();

            fileStopwatch.Stop();

            var scoreStopwatch = Stopwatch.StartNew();

            var matches = files
                .Select(x => new { MatchScore = Score(search, x.Name), Name = x.Name, FullName = x.FullName })
                .Where(x => x.MatchScore.Score != int.MaxValue)
                .ToArray();

            var filteredMatches = matches
                .OrderBy(x => x.MatchScore.Score)
                .ThenBy(x => x.Name.Length)
                .ThenBy(x => x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex)
                .Select(x => new SearchResult(x.Name, x.MatchScore.Score, new FileItem(x.FullName), ImmutableDictionary.Create(Enumerable.Range(x.MatchScore.Range.StartIndex, 1 + x.MatchScore.Range.EndIndex - x.MatchScore.Range.StartIndex).Select(i => new KeyValuePair<int, double>(i, 0.0)))))
                .Take(20)
                .ToArray();

            scoreStopwatch.Stop();

            Log.Info($"Found {matches.Length} results [ {scoreStopwatch.ElapsedMilliseconds + fileStopwatch.ElapsedMilliseconds} ms - scr: {scoreStopwatch.ElapsedMilliseconds} ms, idx: {fileStopwatch.ElapsedMilliseconds} ms ]");

            return filteredMatches;
        }

        public MatchScore Score(string searchString, string targetString)
        {
            var firstSearchChar = searchString[0];
            var restSearchChars = searchString.Substring(1);

            targetString = targetString.ToLowerInvariant();

            var bestScore = int.MaxValue;
            Range bestRange = null;

            for (var i = 0; i < targetString.Length; i++)
            {
                if (targetString[i] != firstSearchChar)
                {
                    continue;
                }

                var score = 1;
                var match = FindEndOfMatch(targetString, restSearchChars, score, i);

                if (match == null)
                {
                    continue;
                }

                if (match.Score < bestScore)
                {
                    bestScore = match.Score;
                    bestRange = match.Range;
                }
            }

            return MatchScore.Create(bestScore, bestRange);
        }

        public class MatchScore
        {
            public Range Range { get; }
            public int Score { get; }

            private MatchScore(int score, Range range)
            {
                Range = range;
                Score = score;
            }

            public static MatchScore Create(int score, Range range)
            {
                return new MatchScore(score, range);
            }
        }

        enum MatchType
        {
            Undefined,
            Normal,
            Sequential,
            Boundary
        }

        private MatchScore FindEndOfMatch(string targetString, string chars, int score, int firstIndex)
        {
            var lastIndex = firstIndex;
            var lastMatch = MatchType.Undefined;

            foreach (var c in chars)
            {
                var index = targetString.IndexOf(c, lastIndex + 1);

                if (index == -1)
                {
                    return null;
                }

                if (index == lastIndex + 1)
                {
                    if (lastMatch != MatchType.Sequential)
                    {
                        score += 1;
                        lastMatch = MatchType.Sequential;
                    }
                }
                else if (!char.IsLetterOrDigit(targetString[index - 1]))
                {
                    if (lastMatch != MatchType.Boundary)
                    {
                        score += 1;
                        lastMatch = MatchType.Boundary;
                    }
                }
                else
                {
                    score += index - lastIndex;
                    lastMatch = MatchType.Normal;
                }

                lastIndex = index;
            }

            return MatchScore.Create(score, new Range(firstIndex, lastIndex));
        }


        public class Range
        {
            public int StartIndex { get; }
            public int EndIndex { get; }

            public Range(int startIndex, int endIndex)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }
        }
    }
}