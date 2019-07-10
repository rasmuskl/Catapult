using System.Linq;
using Catapult.Core.Config;
using Catapult.Core.Indexes;

namespace Catapult.Core.Selecta
{
    public static class ClipboardSearchResources
    {
        private static ClipboardIndexer _clipboardIndexer = new ClipboardIndexer();

        public static ClipboardEntry[] GetClipboardHistory()
        {
            return _clipboardIndexer.ClipboardEntries
                .OrderByDescending(x => x.CreatedUtc)
                .ToArray();
        }

        public static void Dispose()
        {
            _clipboardIndexer?.Dispose();
        }
    }
}