using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AlphaLaunch.Core.Indexes;
using GlobalHotKey;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace AlphaLaunch.App
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;
        private HotKeyManager _hotKeyManager;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            IndexStore.Instance.Start();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;

            _hotKeyManager = new HotKeyManager();

            _hotKeyManager.Register(new HotKey(Key.Space, ModifierKeys.Alt));
            _hotKeyManager.KeyPressed += KeyHookKeyEvent;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AlphaLaunch.App.Icon.ico"))
            {
                if (stream != null)
                {
                    _notifyIcon.Icon = new Icon(stream);
                }
            }

            _notifyIcon.Click += (o, args) => Shutdown();
            _mainWindow = new MainWindow();
        }

        void KeyHookKeyEvent(object sender, KeyPressedEventArgs keyPressedEventArgs)
        {
            if (_mainWindow.Visibility == Visibility.Hidden)
            {
                _mainWindow.ShowDialog();
                _mainWindow.Topmost = true;
            }
            else
            {
                _mainWindow.Hide();
            }
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _notifyIcon.Visible = false;
            _hotKeyManager.Dispose();
        }
    }
}
