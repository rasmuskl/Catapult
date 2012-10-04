using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace AlphaLaunch.App
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AlphaLaunch.App.Icon.ico"))
            {
                if (stream != null)
                {
                    _notifyIcon.Icon = new Icon(stream);
                }
            }

            _notifyIcon.Click += (o, args) => Shutdown();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _notifyIcon.Visible = false;
        }
    }
}
