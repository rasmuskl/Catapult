using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AlphaLaunch.App.KeyHooks
{
    public abstract class AbstractKeyHook : IKeyHook
    {
        protected void RaiseKeyUp(KeyEventArgs args)
        {
            if (KeyUp != null)
                KeyUp(this, args);
        }

        protected void RaiseKeyDown(KeyEventArgs args)
        {
            if (KeyDown != null)
                KeyDown(this, args);
        }

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        public abstract void Install();
        public abstract void Uninstall();
    }
}
