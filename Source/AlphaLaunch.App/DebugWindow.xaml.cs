using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Core.Debug;

namespace AlphaLaunch.App
{
    public partial class DebugWindow
    {
        public DebugWindow()
        {
            InitializeComponent();
            Log.Attach(s => Status.Dispatcher.Invoke(() => Status.Text += s + Environment.NewLine));
        }
    }
}
