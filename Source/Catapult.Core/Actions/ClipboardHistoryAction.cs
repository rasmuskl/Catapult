using System;
using System.Linq;
using System.Windows;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class ClipboardHistoryAction : IndexableBase, IAction<StringIndexable>
    {
        public void Run(StringIndexable stringIndexable)
        {
            Clipboard.SetText(stringIndexable.Name);
        }

        public override string Name => "Clipboard history";

        public ISearchFrame GetSearchFrame()
        {
            var clipboardHistory = SearchResources.GetClipboardHistory()
                .Select(x => new StringIndexable(x.Text, FormatUtcAgo(x.CreatedUtc)))
                .OfType<IIndexable>()
                .ToArray();
            
            return new IndexableSearchFrame(clipboardHistory);
        }

        private static string FormatUtcAgo(DateTime utcDateTime)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan span = now - utcDateTime;

            if (span < TimeSpan.FromMinutes(1))
            {
                return "Now";
            }

            if (now.Date == utcDateTime.AddDays(-1).Date)
            {
                return "Yesterday";
            }

            if (span < TimeSpan.FromHours(1))
            {
                return $"{span.Minutes} minutes ago";
            }

            if (span < TimeSpan.FromDays(1))
            {
                return $"{span.Hours} hours ago";
            }

            return $"{span.Days} days ago";
        }
    }
}