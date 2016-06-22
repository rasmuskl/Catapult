using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Catapult.App.Annotations;
using Catapult.Core;
using Catapult.Core.Config;
using Catapult.Core.Selecta;
using GlobalHotKey;
using Hardcodet.Wpf.TaskbarNotification;
using Serilog;

namespace Catapult.App
{
    public partial class App : IDisposable
    {
        private HotKeyManager _hotKeyManager;
        private MainWindow _mainWindow;
        private TaskbarIcon _taskbarIcon;


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

            _taskbarIcon = (TaskbarIcon) FindResource("MyNotifyIcon");
            InitializeTaskBarIcon(_taskbarIcon);

            _hotKeyManager = new HotKeyManager();

            RegisterHotKey(Key.Space, ModifierKeys.Alt);

            _mainWindow = new MainWindow();

            _mainWindow.IsVisibleChanged += _mainWindow_IsVisibleChanged;

            if (Program.UseSingleLaunchMode)
            {
                ToggleMainWindow();
            }

            SquirrelIntegration.Instance.StartPeriodicUpdateCheck();
        }

        private static void InitializeTaskBarIcon(TaskbarIcon taskbarIcon)
        {
            var taskbarViewModel = new TaskbarViewModel();
            taskbarIcon.DataContext = taskbarViewModel;
            taskbarIcon.ToolTipText = $"Catapult [{AssemblyVersionInformation.Version}]";

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Catapult.App.Icon.ico"))
            {
                if (stream != null)
                {
                    taskbarIcon.Icon = new Icon(stream);
                }
            }

            if (SquirrelIntegration.Instance.NewVersion != null)
            {
                taskbarIcon.ShowBalloonTip("Catapult", $"Updated to new version: {SquirrelIntegration.Instance.NewVersion}", BalloonIcon.None);
            }

            SquirrelIntegration.OnUpdateFound += version =>
            {
                taskbarViewModel.UpgradeVisibility = Visibility.Visible;
                SquirrelIntegration.Instance.UpgradeToNewVersion();
            };
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
            if (!(bool) e.NewValue && Program.UseSingleLaunchMode)
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
                _taskbarIcon.Visibility = Visibility.Hidden;
                _taskbarIcon.Dispose();
                _hotKeyManager.Dispose();
            });
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        private void Upgrade_OnClick(object sender, RoutedEventArgs e)
        {
            SquirrelIntegration.Instance.UpgradeToNewVersion();
        }
    }

    public class TaskbarViewModel : INotifyPropertyChanged
    {
        private Visibility _upgradeVisibility = Visibility.Collapsed;
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility UpgradeVisibility
        {
            get { return _upgradeVisibility; }
            set
            {
                if (value == _upgradeVisibility) return;
                _upgradeVisibility = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}