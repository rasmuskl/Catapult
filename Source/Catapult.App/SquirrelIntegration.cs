using System;
using System.IO;
using System.Threading;
using Catapult.Core;
using Catapult.Core.Actions;
using Serilog;
using Squirrel;

namespace Catapult.App
{
    public class SquirrelIntegration : IDisposable
    {
        public static SquirrelIntegration Instance = new SquirrelIntegration();
        private readonly UpdateManager _updateManager;
        private const string UpdateUrl = "https://github.com/rasmuskl/Catapult";
        public static Action<string> OnUpdateFound;

        private SquirrelIntegration()
        {
            if (!Program.UseSquirrel)
            {
                return;
            }

            _updateManager = UpdateManager.GitHubUpdateManager(UpdateUrl).Result;
        }

        public void HandleSquirrelEvents()
        {
            if (!Program.UseSquirrel)
            {
                return;
            }

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: v =>
                {
                    Log.Information("Squirrel: On initial install: " + v);
                    _updateManager.CreateShortcutForThisExe();
                    EnableRunAtStartUpAction.EnableRunAtStartUp();
                },
                onAppUpdate: v =>
                {
                    File.WriteAllText(CatapultPaths.NewVersionPath, v.ToString());
                    Log.Information("Squirrel: On app update: " + v);
                    _updateManager.CreateShortcutForThisExe();
                    EnableRunAtStartUpAction.UpdateRunAtStartUp();
                },
                onAppUninstall: v =>
                {
                    Log.Information("Squirrel: On app uninstall: " + v);
                    _updateManager.RemoveShortcutForThisExe();
                    DisableRunAtStartUpAction.RemoveRunAtStartUp();
                },
                onAppObsoleted: v => { Log.Information("Squirrel: On app obsoleted: " + v); },
                onFirstRun: () => { Log.Information("Squirrel: On first run."); });
        }

        public void StartPeriodicUpdateCheck()
        {
            var thread = new Thread(o =>
            {
                try
                {
                    CheckForUpdates();
                    Thread.Sleep(TimeSpan.FromHours(3));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Periodic update check failed.");
                }
            }) {IsBackground = true};

            thread.Start();
        }

        public void CheckForUpdates()
        {
            if (!Program.UseSquirrel)
            {
                return;
            }

            Log.Information("Checking for updates.");

            _updateManager.UpdateApp()
                .ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        Log.Error(x.Exception, "Update check failed.");
                        return;
                    }

                    if (x.IsCanceled)
                    {
                        Log.Warning("Updating check was cancelled.");
                        return;
                    }

                    var releaseEntry = x.Result;

                    if (releaseEntry?.Version != null)
                    {
                        Log.Information("Downloaded new version: " + releaseEntry.Version);
                        OnUpdateFound?.Invoke(releaseEntry.Version.ToString());
                    }

                    Log.Information("Update check complete.");
                });
        }

        public void UpgradeToNewVersion()
        {
            if (!Program.UseSquirrel)
            {
                return;
            }

            Program.CleanUp();
            UpdateManager.RestartApp();
        }

        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }
}