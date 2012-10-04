using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace AlphaLaunch.App.KeyHooks
{
    public interface IKeyHook
    {
        event KeyEventHandler KeyDown;
        event KeyEventHandler KeyUp;
        void Install();
        void Uninstall();
    }
}