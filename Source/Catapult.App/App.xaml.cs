using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Catapult.Core;
using Catapult.Core.Config;
using Catapult.Core.Selecta;
using Hardcodet.Wpf.TaskbarNotification;
using JetBrains.Annotations;
using NHotkey;
using NHotkey.Wpf;
using Serilog;

namespace Catapult.App;

public partial class App : IDisposable
{
    // private HotKeyManager _hotKeyManager;
    private MainWindow _mainWindow;
    private TaskbarIcon? _taskbarIcon;

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        Log.Information("Started Catapult");

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

        _taskbarIcon = FindResource("MyNotifyIcon") as TaskbarIcon;
        InitializeTaskBarIcon(_taskbarIcon);

        // _hotKeyManager = new HotKeyManager();
        RegisterHotKey(Key.Space, configuration.UseControlKey ? ModifierKeys.Control : ModifierKeys.Alt);

        _mainWindow = new MainWindow();

        _mainWindow.IsVisibleChanged += _mainWindow_IsVisibleChanged;

        if (Program.UseSingleLaunchMode)
        {
            ToggleMainWindow();
        }
    }

    private static void InitializeTaskBarIcon(TaskbarIcon? taskbarIcon)
    {
        if (taskbarIcon is null)
        {
            throw new InvalidOperationException("TaskbarIcon is null");
        }
            
        var taskbarViewModel = new TaskbarViewModel();
        taskbarIcon.DataContext = taskbarViewModel;
        taskbarIcon.ToolTipText = "Catapult";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Catapult.App.Icon.ico");
            
        if (stream != null)
        {
            taskbarIcon.Icon = new Icon(stream);
        }
    }

    private void RegisterHotKey(Key key, ModifierKeys modifierKeys)
    {
        if (Program.UseSingleLaunchMode)
        {
            return;
        }

        Log.Information("Registering hot key");
        HotkeyManager.Current.AddOrReplace("Toggle", key, modifierKeys, KeyHookKeyEvent);
        // var hotKey = new HotKey(key, modifierKeys);
        // _hotKeyManager.Register(hotKey);
        // _hotKeyManager.KeyPressed += KeyHookKeyEvent;
    }

    private void _mainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool) e.NewValue && Program.UseSingleLaunchMode)
        {
            _mainWindow.Model.AddIntent(new ShutdownIntent(Shutdown));
        }
    }

    private void KeyHookKeyEvent(object? sender, HotkeyEventArgs hotkeyEventArgs)
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
            if (_taskbarIcon == null)
            {
                return;
            }
                
            _taskbarIcon.Visibility = Visibility.Hidden;
            _taskbarIcon.Dispose();
        });
    }

    private void Exit_OnClick(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }
}

public sealed class TaskbarViewModel : INotifyPropertyChanged
{
    private Visibility _upgradeVisibility = Visibility.Collapsed;
    public event PropertyChangedEventHandler? PropertyChanged;

    public Visibility UpgradeVisibility
    {
        get => _upgradeVisibility;
        set
        {
            if (value == _upgradeVisibility) return;
            _upgradeVisibility = value;
            OnPropertyChanged();
        }
    }

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}