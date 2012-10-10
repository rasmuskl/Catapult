using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.App.Debug;

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
