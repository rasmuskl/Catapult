using System.Windows.Forms;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class UnderscorizeClipboardString : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            var text = Clipboard.GetText();
            var underscorizedText = text.Replace(" ", "_");
            Clipboard.SetText(underscorizedText);
        }

        public override string Name => "Underscorize clipboard string";
    }
}