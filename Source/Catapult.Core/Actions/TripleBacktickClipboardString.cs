using System;
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

            var plingifiedText = $"```{Environment.NewLine}{text}{Environment.NewLine}```";
            TextCopy.Clipboard.SetText(plingifiedText);
        }

        public override string Name => "Triple backtick clipboard string";
    }
}