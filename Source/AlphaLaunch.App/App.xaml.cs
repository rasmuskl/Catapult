using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AlphaLaunch.Core.Indexes;
using GlobalHotKey;

namespace AlphaLaunch.App
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;
        private HotKeyManager _hotKeyManager;
        private DebugWindow _debugWindow;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

            _mainWindow.IsVisibleChanged += _mainWindow_IsVisibleChanged;

            _debugWindow = new DebugWindow();

#if DEBUG
            ToggleMainWindow();
#endif
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                return;
            }

            System.Windows.MessageBox.Show(exception.Message 
                + Environment.NewLine
                + Environment.NewLine 
                + exception.StackTrace, "Exception occured");
        }

        void _mainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                _debugWindow.Show();
            }
            else
            {
                _debugWindow.Hide();

#if DEBUG
                Shutdown();
#endif
            }
        }

        void KeyHookKeyEvent(object sender, KeyPressedEventArgs keyPressedEventArgs)
        {
            ToggleMainWindow();
        }

        private void ToggleMainWindow()
        {
            if (_mainWindow.Visibility != Visibility.Visible)
            {
                _mainWindow.Show();
                _mainWindow.Topmost = true;

                _debugWindow.Left = _mainWindow.Left + _mainWindow.Width + 20;
                _debugWindow.Top = (SystemParameters.PrimaryScreenHeight - _debugWindow.Height)/2;
                _debugWindow.Topmost = true;
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
