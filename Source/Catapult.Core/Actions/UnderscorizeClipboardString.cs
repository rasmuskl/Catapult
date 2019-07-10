using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class UnderscorizeClipboardString : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            var text = TextCopy.Clipboard.GetText();

            if (text.IsNullOrWhiteSpace())
            {
                return;
            }

            var underscorizedText = text.Replace(" ", "_");
            TextCopy.Clipboard.SetText(underscorizedText);
        }

        public override string Name => "Underscorize clipboard string";
    }
}