using System.Net.Http;
using Catapult.Core.Indexes;
using Catapult.Core.Services;
using Serilog;

namespace Catapult.Core.Actions;

public class CheckForUpdatesAction : IndexableBase, IStandaloneAction
{
    private static readonly HttpClient HttpClient = new();
    private static readonly UpdateService UpdateService = new(HttpClient, Log.ForContext<UpdateService>());

    public override string Name => "Check for Updates";
    public override string? Details => "Check for available application updates";

    public async void Run()
    {
        try
        {
            var updateInfo = await UpdateService.CheckForUpdatesAsync();
            
            if (updateInfo != null)
            {
                Log.Information("Update available: v{Version}", updateInfo.LatestVersion);
                // In a real implementation, you'd show a notification or dialog
                // For now, we'll just log the update availability
                await UpdateService.InstallUpdateAsync(updateInfo.DownloadUrl);
            }
            else
            {
                Log.Information("No updates available");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates");
        }
    }
}

