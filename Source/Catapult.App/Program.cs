using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Catapult.Core;
using Catapult.Core.Debug;
using Catapult.Core.Selecta;
using Serilog;

namespace Catapult.App
{
    public class Program
    {
        public static bool UseSquirrel = !IsDebug();
        public static bool UseSingleLaunchMode = IsDebug();
        private static App _app;
        private static bool _cleanedUp = false;

        [STAThread]
        public static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            var logger = new LoggerConfiguration()
                .WriteTo.RollingFile(Path.Combine(CatapultPaths.LogPath, "log-{Date}.log"))
                .WriteTo.LogWindow()
                .CreateLogger();

            Log.Logger = logger;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            SquirrelIntegration.Instance.HandleSquirrelEvents();

            _app = new App();

            _app.Exit += App_Exit; ;
            _app.DispatcherUnhandledException += App_DispatcherUnhandledException;

            _app.InitializeComponent();
            _app.Run();
        }

        private static void App_Exit(object sender, ExitEventArgs e)
        {
            Log.Information("Exitting Catapult (App_Exit).");
            CleanUp();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Log.Information("Exitting Catapult (CurrentDomain_ProcessExit).");
            CleanUp();
        }

        public static void CleanUp()
        {
            if (_cleanedUp)
            {
                return;
            }

            Log.Information("Clean-up starting.");

            _app?.Dispose();
            SearchResources.Dispose();
            SquirrelIntegration.Instance.Dispose();

            _cleanedUp = true;
            Log.Information("Clean-up complete.");
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteToDisk("Error-CurrentDomain_UnhandledException", e.ExceptionObject as Exception);

            Log.Information("Unhandled exception.");

            var exception = e.ExceptionObject as Exception;

            if (exception == null)
            {
                return;
            }

            Log.Error(exception, "Unhandled exception");

            MessageBox.Show(exception.Message
                            + Environment.NewLine
                            + Environment.NewLine
                            + exception.StackTrace, "Exception occured");
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteToDisk("Error-App_DispatcherUnhandledException", e.Exception);

            Log.Information("Unhandled exception (dispatcher).");

            var exception = e.Exception;

            if (exception == null)
            {
                return;
            }

            e.Handled = true;

            Log.Error(exception, "Unhandled exception (dispatcher)");

            MessageBox.Show(exception.Message
                            + Environment.NewLine
                            + Environment.NewLine
                            + exception.StackTrace, "Exception occured (dispatcher)");
        }

        private static void WriteToDisk(string title, Exception exception)
        {
            using (var stream = File.Create(Path.Combine(CatapultPaths.LogPath, $"error-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}-{Guid.NewGuid()}.txt")))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.WriteLine(title);

                    if (exception != null)
                    {
                        streamWriter.WriteLine(exception.ToString());
                    }
                }

                stream.Flush(true);
            }
        }

        private static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}