using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Catapult.Core;
using Catapult.Core.Config;
using Catapult.Core.Selecta;
using GlobalHotKey;
using Serilog;

namespace Catapult.App
{
    public partial class App : IDisposable
    {
        private HotKeyManager _hotKeyManager;
        private MainWindow _mainWindow;
        private NotifyIcon _notifyIcon;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Log.Information("Started Catapult.");

            if (!Directory.Exists(CatapultPaths.DataPath))
            {
                Directory.CreateDirectory(CatapultPaths.DataPath);
            }

            var loader = new JsonConfigLoader();

            var configuration = loader.LoadUserConfig(CatapultPaths.ConfigPath);
            loader.SaveUserConfig(configuration, CatapultPaths.ConfigPath);

            Task.Factory.StartNew(() =>
            {
                SearchResources.SetConfig(configuration);
                SearchResources.GetFiles();
            });

            _notifyIcon = new NotifyIcon { Visible = true };
            _hotKeyManager = new HotKeyManager();

            RegisterHotKey(Key.Space, ModifierKeys.Alt);

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Catapult.App.Icon.ico"))
            {
                if (stream != null)
                {
                    _notifyIcon.Icon = new Icon(stream);
                }
            }

            _notifyIcon.Click += (o, args) => Shutdown();
            _mainWindow = new MainWindow();

            _mainWindow.IsVisibleChanged += _mainWindow_IsVisibleChanged;

            if (Program.UseSingleLaunchMode)
            {
                ToggleMainWindow();
            }

            SquirrelIntegration.Instance.CheckForUpdates();
        }

        private void RegisterHotKey(Key key, ModifierKeys modifierKeys)
        {
            if (Program.UseSingleLaunchMode)
            {
                return;
            }

            Log.Information("Registering hot key.");
            var hotKey = new HotKey(key, modifierKeys);
            _hotKeyManager.Register(hotKey);
            _hotKeyManager.KeyPressed += KeyHookKeyEvent;
        }

        private void _mainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue && Program.UseSingleLaunchMode)
            {
                _mainWindow.Model.AddIntent(new ShutdownIntent(Shutdown));
            }
        }

        private void KeyHookKeyEvent(object sender, KeyPressedEventArgs keyPressedEventArgs)
        {
            ToggleMainWindow();
        }

        private void ToggleMainWindow()
        {
            if (_mainWindow.Visibility != Visibility.Visible)
            {
                _mainWindow.Show();
                _mainWindow.Topmost = true;

                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.Hide();
            }
        }

        public void Dispose()
        {
            Dispatcher.Invoke(() =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _hotKeyManager.Dispose();
            });
        }
    }
}