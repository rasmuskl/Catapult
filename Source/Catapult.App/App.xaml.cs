using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Catapult.Core;
using Catapult.Core.Config;
using Catapult.Core.Debug;
using Catapult.Core.Selecta;
using GlobalHotKey;
using Serilog;

namespace Catapult.App
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;
        private HotKeyManager _hotKeyManager;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;

            var logger = new LoggerConfiguration()
                .WriteTo.RollingFile(Path.Combine(CatapultPaths.LogPath, "log-{Date}.log"))
                .WriteTo.LogWindow()
                .CreateLogger();

            Log.Logger = logger;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

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

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Visible = true;

            _hotKeyManager = new HotKeyManager();

#if !DEBUG
            _hotKeyManager.Register(new HotKey(Key.Space, ModifierKeys.Alt));
            _hotKeyManager.KeyPressed += KeyHookKeyEvent;
#endif

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

#if DEBUG
            ToggleMainWindow();
#endif
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;

            if (exception == null)
            {
                return;
            }

            e.Handled = true;

            Log.Error(exception, "Unhandled exception (dispatcher)");

            System.Windows.MessageBox.Show(exception.Message
                + Environment.NewLine
                + Environment.NewLine
                + exception.StackTrace, "Exception occured (dispatcher)");
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                return;
            }

            Log.Error(exception, "Unhandled exception");

            System.Windows.MessageBox.Show(exception.Message
                + Environment.NewLine
                + Environment.NewLine
                + exception.StackTrace, "Exception occured");
        }

        void _mainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
#if DEBUG
                _mainWindow.Model.AddIntent(new ShutdownIntent(Shutdown));
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
                
                _mainWindow.Activate();
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
            SearchResources.Dispose();
        }
    }
}
