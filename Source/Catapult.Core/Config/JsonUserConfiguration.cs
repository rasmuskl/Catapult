namespace Catapult.Core.Config;

public class JsonUserConfiguration
{
    public string[] Paths { get; set; } = [];
    public string[] IgnoredDirectories { get; set; } = [];
    public string[] Extensions { get; set; } = [];
    public bool UseControlKey { get; set; }

    public static JsonUserConfiguration BuildDefaultSettings()
    {
        return new JsonUserConfiguration
        {
            Paths =
            [
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"c:\dev"
            ],
            IgnoredDirectories = ["node_modules", ".git", "scratch"],
            Extensions = [".lnk", ".exe", ".sln", ".slnx", ".slnf", ".code-workspace", ".url", ".docx", ".xlsx", ".pptx", ".pdf"],
            UseControlKey = false
        };
    }
}