using System.Diagnostics;
using System.Drawing;
using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core;

public class ControlPanelItem : IndexableBase, IStandaloneAction
{
    private readonly ProcessStartInfo _startInfo;
    private readonly Icon _icon;

    public ControlPanelItem(string name, ProcessStartInfo startInfo, string infoTip, Icon icon)
    {
        Name = name;
        _startInfo = startInfo;
        Details = infoTip;
        _icon = icon;
    }

    public override string Name { get; }

    public override IIconResolver GetIconResolver()
    {
        return new StaticIconResolver(_icon);
    }

    public override string Details { get; }

    public void Run()
    {
        Process.Start(_startInfo);
    }
}