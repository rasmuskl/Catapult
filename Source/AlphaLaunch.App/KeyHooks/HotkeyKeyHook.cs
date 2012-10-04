using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AlphaLaunch.App.KeyHooks
{
    public class HotkeyKeyHook : AbstractKeyHook
    {
        private readonly ModKeys _modKeys;
        private readonly System.Windows.Forms.Keys _key;
        private readonly GlobalKBHook _internalHook;

        public HotkeyKeyHook(ModKeys modKeys, System.Windows.Forms.Keys key)
        {
            _modKeys = modKeys;
            _key = key;
            _internalHook = new GlobalKBHook();
            _internalHook.KBHookInvoked += InternalHookKbHookInvoked;
        }

        void InternalHookKbHookInvoked(object sender, KBHookEventArgs e)
        {
            //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
            //ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);
            var key = (System.Windows.Forms.Keys) Enum.Parse(typeof (System.Windows.Forms.Keys), e.lParam.vkCode.ToString());

            if(key == _key)
            {
                var args = new KeyEventArgs(key);

                if(e.IsKeyDown())
                    RaiseKeyUp(args);
                else 
                    RaiseKeyDown(args);

                e.AbortKey = true;
            }
        }

        public override void Install()
        {
            _internalHook.Install();
        }

        public override void Uninstall()
        {
            _internalHook.Uninstall();
        }
    }

    [Flags]
    public enum ModKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
