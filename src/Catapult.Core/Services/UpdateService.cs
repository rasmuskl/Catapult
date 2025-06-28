using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;

namespace Catapult.Core.Services;

public class UpdateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private const string GitHubApiUrl = "https://api.github.com/repos/rasmuskl/Catapult/releases/latest";
    private const string AppInstallerUrl = "https://github.com/rasmuskl/Catapult/releases/latest/download/Catapult.appinstaller";

    public UpdateService(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Catapult/1.0");
            
            var response = await _httpClient.GetAsync(GitHubApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Failed to check for updates: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var release = JsonConvert.DeserializeObject<GitHubRelease>(json);
            
            if (release == null)
            {
                _logger.Warning("Failed to parse GitHub release response");
                return null;
            }

            var currentVersion = GetCurrentVersion();
            var latestVersion = ParseVersion(release.TagName);
            
            if (latestVersion > currentVersion)
            {
                return new UpdateInfo
                {
                    LatestVersion = latestVersion,
                    ReleaseNotes = release.Body,
                    DownloadUrl = AppInstallerUrl,
                    PublishedAt = release.PublishedAt
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for updates");
            return null;
        }
    }

    public async Task<bool> InstallUpdateAsync(string appInstallerUrl)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ms-appinstaller",
                Arguments = $"?source={Uri.EscapeDataString(appInstallerUrl)}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start update installation");
            return false;
        }
    }

    private static Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version ?? new Version(1, 0, 0, 0);
    }

    private static Version ParseVersion(string tagName)
    {
        // Remove 'v' prefix if present
        var versionString = tagName.StartsWith('v') ? tagName[1..] : tagName;
        
        // Ensure 4-part version for MSIX compatibility
        if (Version.TryParse(versionString, out var version))
        {
            return new Version(version.Major, version.Minor, version.Build == -1 ? 0 : version.Build, version.Revision == -1 ? 0 : version.Revision);
        }
        
        return new Version(1, 0, 0, 0);
    }
}

public class UpdateInfo
{
    public Version LatestVersion { get; set; } = new(1, 0, 0, 0);
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}

internal class GitHubRelease
{
    [JsonProperty("tag_name")]
    public string TagName { get; set; } = string.Empty;
    
    [JsonProperty("body")]
    public string Body { get; set; } = string.Empty;
    
    [JsonProperty("published_at")]
    public DateTime PublishedAt { get; set; }
}