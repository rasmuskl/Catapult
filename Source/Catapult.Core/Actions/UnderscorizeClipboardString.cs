using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class UnderscorizeClipboardString : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        var text = new TextCopy.Clipboard().GetText();

        if (text.IsNullOrWhiteSpace())
        {
            return;
        }

        var underscorizedText = text.Replace(" ", "_");
        new TextCopy.Clipboard().SetText(underscorizedText);
    }

    public override string Name => "Underscorize clipboard string";
}