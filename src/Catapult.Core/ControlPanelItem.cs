using System.Diagnostics;
using System.Drawing;
using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core;

public class ControlPanelItem(string name, ProcessStartInfo startInfo, string infoTip, Icon icon)
    : IndexableBase, IStandaloneAction
{
    public override string Name { get; } = name;

    public override IIconResolver GetIconResolver()
    {
        return new StaticIconResolver(icon);
    }

    public override string Details { get; } = infoTip;

    public void Run()
    {
        Process.Start(startInfo);
    }
}