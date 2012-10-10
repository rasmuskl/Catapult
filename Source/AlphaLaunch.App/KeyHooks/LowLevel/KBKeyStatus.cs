using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App.KeyHooks.LowLevel
{
    public enum KBKeyStatus
    {
        KeyUp = 0x101,
        SysKeyUp = 0x105,
        KeyDown = 0x100,
        SysKeyDown = 0x104,
    }
}