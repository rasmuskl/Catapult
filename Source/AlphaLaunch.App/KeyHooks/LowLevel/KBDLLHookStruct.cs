using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App.KeyHooks.LowLevel
{
    public struct KBDLLHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }
}