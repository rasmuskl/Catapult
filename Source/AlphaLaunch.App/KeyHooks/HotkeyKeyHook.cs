using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaLaunch.App.KeyHooks.LowLevel;

namespace AlphaLaunch.App.KeyHooks
{
    public class HotkeyKeyHook : AbstractKeyHook
    {
        private readonly ModKeys _modKeys;
        private readonly Keys _key;
        private readonly GlobalKBHook _internalHook;
        private bool _modKeyDown;
        private bool _keyDown;

        public HotkeyKeyHook(ModKeys modKeys, Keys key)
        {
            _modKeys = modKeys;

            if (_modKeys != ModKeys.Win)
            {
                throw new ArgumentException("Only WIN is supported for hotkeys for now.", "modKeys");
            }

            _key = key;
            _internalHook = new GlobalKBHook();
            _internalHook.KBHookInvoked += InternalHookKbHookInvoked;
        }

        void InternalHookKbHookInvoked(object sender, KBHookEventArgs e)
        {
            var key = (Keys)Enum.Parse(typeof(Keys), e.lParam.vkCode.ToString());
            
            if(key == Keys.LMenu || key == Keys.RMenu)
            {
                _modKeyDown = e.IsKeyDown();
            }

            if (key != _key)
            {
                return;
            }

            if (_keyDown && e.IsKeyUp())
            {
                e.AbortKey = true;
                _keyDown = false;
                RaiseKeyUp(new KeyEventArgs(key));
            }

            if (!_keyDown && _modKeyDown && e.IsKeyDown())
            {
                e.AbortKey = true;
                _keyDown = true;
                RaiseKeyDown(new KeyEventArgs(key));
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
