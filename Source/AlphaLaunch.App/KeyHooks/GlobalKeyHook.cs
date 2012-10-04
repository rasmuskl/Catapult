using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AlphaLaunch.App.KeyHooks
{
    public class GlobalKeyHook : AbstractKeyHook
    {
        private readonly bool _suppressAll;
        private readonly GlobalKBHook _internalHook;

        public GlobalKeyHook(bool suppressAll)
        {
            _suppressAll = suppressAll;
            _internalHook = new GlobalKBHook();

            _internalHook.KBHookInvoked += InternalHookKbHookInvoked;
        }

        private void InternalHookKbHookInvoked(object sender, KBHookEventArgs e)
        {
            var key = (System.Windows.Forms.Keys) Enum.Parse(typeof (System.Windows.Forms.Keys), e.lParam.vkCode.ToString());
            var args = new KeyEventArgs(key);

            args.SuppressKeyPress = _suppressAll;

            if (e.IsKeyDown())
                RaiseKeyDown(args);

            if (e.IsKeyUp())
                RaiseKeyUp(args);

            e.AbortKey = args.SuppressKeyPress;
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
}