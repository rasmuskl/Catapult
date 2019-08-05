using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging.Serilog;
using Catapult.AvaloniaApp.ViewModels;
using Catapult.AvaloniaApp.Views;
using Catapult.Core;
using Catapult.Core.Config;
using Catapult.Core.Selecta;
using Serilog;

namespace Catapult.AvaloniaApp
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            Log.Information("Started Catapult.");

            if(SocketActivation.TryConnect())
            {
                return;
            }

            SocketActivation.CreateServer();

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

            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            app.Run(window);
        }

        public static bool UseSingleLaunchMode { get; set; }
    }
}
