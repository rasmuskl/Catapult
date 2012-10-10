using System.Linq;
using System.Collections.Generic;
using System;

namespace AlphaLaunch.App.KeyHooks.LowLevel
{
    public class KBHookEventArgs : EventArgs
    {
        public int HookCode;
        public IntPtr wParam;
        public KBDLLHookStruct lParam;
        public bool AbortKey = false;
        public KBKeyStatus KeyStatus;

        public bool IsKeyDown()
        {
            return KeyStatus == KBKeyStatus.KeyDown || KeyStatus == KBKeyStatus.SysKeyDown;
        }

        public bool IsKeyUp()
        {
            return KeyStatus == KBKeyStatus.KeyUp || KeyStatus == KBKeyStatus.SysKeyUp;
        }
    }
}