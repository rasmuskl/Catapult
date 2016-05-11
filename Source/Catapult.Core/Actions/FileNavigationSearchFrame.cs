using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Catapult.Core.Frecency;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class FileNavigationSearchFrame : ISearchFrame
    {
        private Searcher _searcher;
        private IIndexable _primaryIndexable = null;

        public FileNavigationSearchFrame(string directoryName, string primaryPath)
        {
            var fileSystemInfos = new DirectoryInfo(directoryName).GetFileSystemInfos();

            var indexables = new List<IIndexable>();

            foreach (var fileSystemInfo in fileSystemInfos)
            {
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    var folderItem = new FolderItem(fileSystemInfo.FullName);

                    if (fileSystemInfo.FullName == primaryPath)
                    {
                        _primaryIndexable = folderItem;
                    }

                    indexables.Add(folderItem);
                }
                else
                {
                    var fileItem = new FileItem(fileSystemInfo.FullName);

                    if (fileSystemInfo.FullName == primaryPath)
                    {
                        _primaryIndexable = fileItem;
                    }

                    indexables.Add(fileItem);
                }
            }

            _searcher = Searcher.Create(indexables.ToArray());
        }

        public SearchResult[] PerformSearch(string search, FrecencyStorage frecencyStorage)
        {
            _searcher = _searcher.Search(search);

            if (string.IsNullOrWhiteSpace(search) && _primaryIndexable != null)
            {
                var primarySearchResult = new SearchResult(_primaryIndexable.Name, 0, _primaryIndexable, ImmutableHashSet.Create<int>());
                return new[] { primarySearchResult }.Concat(_searcher.SearchResults.Where(x => x.TargetItem != _primaryIndexable)).ToArray();
            }

            return _searcher.SearchResults;
        }
    }
}