using System;
using System.Windows.Forms;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class PlingifyClipboardString : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            var text = Clipboard.GetText();
            var plingifiedText = "```" + Environment.NewLine + text + Environment.NewLine + "```";
            Clipboard.SetText(plingifiedText);
        }

        public override string Name => "Plingify clipboard string";
    }
}