using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using AlphaLaunch.App.KeyHooks;

namespace AlphaLaunch.App
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;
        private HotkeyKeyHook _keyHook;
        private MainWindow _mainWindow;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;

            _keyHook = new HotkeyKeyHook(ModKeys.None, Keys.CapsLock);
            _keyHook.KeyDown += KeyHookKeyEvent;
            _keyHook.Install();

            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AlphaLaunch.App.Icon.ico"))
            {
                if (stream != null)
                {
                    _notifyIcon.Icon = new Icon(stream);
                }
            }

            _notifyIcon.Click += (o, args) => Shutdown();

            _mainWindow = new MainWindow();
        }

        void KeyHookKeyEvent(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            _mainWindow.Show(); 
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _notifyIcon.Visible = false;
            _keyHook.Uninstall();
        }
    }
}
