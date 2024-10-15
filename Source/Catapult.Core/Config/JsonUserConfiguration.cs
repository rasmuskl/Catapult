namespace Catapult.Core.Config;

public class JsonUserConfiguration
{
    public JsonUserConfiguration()
    {
        Paths = new string[0];
        IgnoredDirectories = new string[0];
        Extensions = new string[0];
    }

    public string[] Paths { get; set; }
    public string[] IgnoredDirectories { get; set; }
    public string[] Extensions { get; set; }
    public bool UseControlKey { get; set; }

    public static JsonUserConfiguration BuildDefaultSettings()
    {
        return new JsonUserConfiguration
        {
            Paths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"c:\dev",
            },
            IgnoredDirectories = new[] { "node_modules", ".git", "scratch" },
            Extensions = new[] { ".lnk", ".exe", ".sln", ".url", ".docx", ".xlsx", ".pptx", ".pdf" },
            UseControlKey = false
        };
    }
}