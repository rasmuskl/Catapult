using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public class CopyClipboardAction(string name, string details) 
    : IndexableBase, IStandaloneAction
{
    public override string Name => name;
    public override string Details => details;
    public override string BoostIdentifier => string.Empty;
    
    public void Run()
    {
        new TextCopy.Clipboard().SetText(Name);
    }
}