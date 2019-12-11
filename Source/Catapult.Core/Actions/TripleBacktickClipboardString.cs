using System;
using System.Collections.Generic;
using System.Linq;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class TripleBacktickClipboardString : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            var text = TextCopy.Clipboard.GetText();

            if (text.IsNullOrWhiteSpace())
            {
                return;
            }

            var plingifiedText = $"```{Environment.NewLine}{TrimSharedWhitespacePrefix(text)}{Environment.NewLine}```";
            TextCopy.Clipboard.SetText(plingifiedText);
        }

        public override string Name => "Triple backtick clipboard string";

        public static string TrimSharedWhitespacePrefix(string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return str;
            }

            IEnumerable<string> lines = str.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
            var min = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Min(x => x.TakeWhile(char.IsWhiteSpace).Count());
            lines = lines.Select(x => !string.IsNullOrWhiteSpace(x) ? x.Substring(min) : x);
            return string.Join(Environment.NewLine, lines);
        }
    }
}