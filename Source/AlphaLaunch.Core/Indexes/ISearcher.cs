using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Core.Indexes
{
    public interface ISearcher
    {
        void IndexItems(FileItem[] items);
        IEnumerable<SearchResult> Search(string search);
    }
}