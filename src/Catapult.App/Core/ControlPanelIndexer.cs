using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Catapult.Core;
using Microsoft.Win32;

namespace Catapult.App.Core;

public class ControlPanelIndexer
{
    private static readonly RegistryKey ControlPanelNameSpace = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ControlPanel\\NameSpace") 
        ?? throw new InvalidOperationException("Failed to open Control Panel NameSpace registry key.");

    private static readonly RegistryKey Clsid = Registry.ClassesRoot.OpenSubKey("CLSID") 
        ?? throw new InvalidOperationException("Failed to open CLSID registry key.");
    
    private const string Control = @"%SystemRoot%\System32\control.exe";

    public ControlPanelItem[] GetControlPanelItems()
    {
        var items = new List<ControlPanelItem>();

        var subKeyNames = ControlPanelNameSpace.GetSubKeyNames();

        foreach (var subKey in subKeyNames)
        {
            using (var currentKey = Clsid.OpenSubKey(subKey))
            {
                if (currentKey == null)
                {
                    continue;
                }

                var executablePath = GetExecutablePath(currentKey);
                var localizedString = GetLocalizedString(currentKey);

                if (string.IsNullOrEmpty(localizedString))
                {
                    continue;
                }

                var infoTip = GetInfoTip(currentKey);
                var myIcon = GetIcon(currentKey, 32);


                if (executablePath != null && infoTip != null && myIcon != null)
                {
                    items.Add(new ControlPanelItem(localizedString, executablePath, infoTip, myIcon));
                }
            }
        }

        return items.ToArray();
    }

    private static ProcessStartInfo? GetExecutablePath(RegistryKey currentKey)
    {
        if (currentKey.GetValue("System.ApplicationName") != null)
        {
            ProcessStartInfo executablePath = new ProcessStartInfo();

            var applicationName = currentKey.GetValue("System.ApplicationName")?.ToString();
            executablePath.FileName = Environment.ExpandEnvironmentVariables(Control);
            executablePath.Arguments = "-name " + applicationName;

            return executablePath;
        }

        using (var subKey = currentKey.OpenSubKey("Shell\\Open\\Command"))
        {
            if (subKey?.GetValue(null) != null)
            {
                var executablePath = new ProcessStartInfo();

                var input = $"\"{Environment.ExpandEnvironmentVariables(subKey.GetValue(null)?.ToString() ?? string.Empty)}\"";
                executablePath.FileName = "cmd.exe";
                executablePath.Arguments = "/C " + input;
                executablePath.WindowStyle = ProcessWindowStyle.Hidden;

                return executablePath;
            }
        }
        return null;
    }

    private string? GetLocalizedString(RegistryKey currentKey)
    {
        if (currentKey.GetValue("LocalizedString") != null)
        {
            var localizedStringRaw = currentKey.GetValue("LocalizedString")
                ?.ToString()
                ?.Split([",-"], StringSplitOptions.None);

            if (localizedStringRaw is { Length: > 1 })
            {
                if (localizedStringRaw[0][0] == '@')
                {
                    localizedStringRaw[0] = localizedStringRaw[0].Substring(1);
                }

                localizedStringRaw[0] = Environment.ExpandEnvironmentVariables(localizedStringRaw[0]);

                var dataFilePointer = LoadLibraryEx(localizedStringRaw[0], IntPtr.Zero, LoadLibraryAsDatafile);

                var stringTableIndex = ParseIndex(localizedStringRaw[1]);

                var resource = new StringBuilder(255);
                LoadString(dataFilePointer, stringTableIndex, resource, resource.Capacity + 1);
                FreeLibrary(dataFilePointer);

                var localizedString = resource.ToString();

                if (localizedString.IsSet())
                {
                    return localizedString;
                }

                if (currentKey.GetValue(null) != null)
                {
                    return currentKey.GetValue(null)?.ToString();
                }

                return null;
            }

            return localizedStringRaw?[0];
        }

        if (currentKey.GetValue(null) != null)
        {
            return currentKey.GetValue(null)?.ToString();
        }

        return null;
    }

    private uint ParseIndex(string indexStr)
    {
        return uint.Parse(Regex.Match(indexStr, @"(?<num>\d+)").Value);
    }

    private string? GetInfoTip(RegistryKey currentKey)
    {
        if (currentKey.GetValue("InfoTip") != null)
        {
            var infoTipRaw = currentKey.GetValue("InfoTip")?.ToString()?.Split([",-"], StringSplitOptions.None);

            if (infoTipRaw is { Length: 2 })
            {
                if (infoTipRaw[0][0] == '@')
                {
                    infoTipRaw[0] = infoTipRaw[0].Substring(1);
                }
                infoTipRaw[0] = Environment.ExpandEnvironmentVariables(infoTipRaw[0]);

                var dataFilePointer = LoadLibraryEx(infoTipRaw[0], IntPtr.Zero, LoadLibraryAsDatafile);

                var stringTableIndex = ParseIndex(infoTipRaw[1]);

                var resource = new StringBuilder(255);
                LoadString(dataFilePointer, stringTableIndex, resource, resource.Capacity + 1);
                FreeLibrary(dataFilePointer);

                return resource.ToString();
            }

            return currentKey.GetValue("InfoTip")?.ToString();
        }

        return null;
    }

    private delegate bool EnumResNameDelegate(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

    [DllImport("kernel32.dll", EntryPoint = "EnumResourceNamesW", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool EnumResourceNamesWithID(IntPtr hModule, uint lpszType, EnumResNameDelegate lpEnumFunc,
        IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int LoadString(IntPtr hInstance, uint uId, StringBuilder lpBuffer, int nBufferMax);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr LoadImage(IntPtr hinst, IntPtr lpszName, uint uType, int cxDesired, int cyDesired,
        uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool DestroyIcon(IntPtr handle);

    private const uint GroupIcon = 14;
    private const uint LoadLibraryAsDatafile = 0x00000002;

    private Icon? GetIcon(RegistryKey currentKey, int iconSize)
    {
        using (var subKey = currentKey.OpenSubKey("DefaultIcon"))
        {
            if (subKey?.GetValue(null) == null)
            {
                return null;
            }

            var iconString = subKey.GetValue(null)?.ToString()?.Split([','], 2) ?? [];

            if (iconString[0][0] == '@')
            {
                iconString[0] = iconString[0].Substring(1);
            }

            IntPtr dataFilePointer = LoadLibraryEx(iconString[0], IntPtr.Zero, LoadLibraryAsDatafile);
            IntPtr iconPtr = IntPtr.Zero;

            if (iconString.Length == 2)
            {
                var iconIndex = (IntPtr) ParseIndex(iconString[1]);
                iconPtr = LoadImage(dataFilePointer, iconIndex, 1, iconSize, iconSize, 0);
            }

            if (iconPtr == IntPtr.Zero)
            {
                var defaultIconPtr = IntPtr.Zero;
                EnumResourceNamesWithID(dataFilePointer, GroupIcon, (_, b, _, _) =>
                {
                    defaultIconPtr = b;
                    return false;
                }, IntPtr.Zero);
                iconPtr = LoadImage(dataFilePointer, defaultIconPtr, 1, iconSize, iconSize, 0);
            }

            FreeLibrary(dataFilePointer);

            return iconPtr.ExtractIconAndDestroyIconPointer();
        }
    }
}

public static class IntPtrExtensions
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool DestroyIcon(IntPtr handle);

    public static Icon? ExtractIconAndDestroyIconPointer(this IntPtr iconPtr)
    {
        if (iconPtr == IntPtr.Zero)
        {
            return null;
        }

        Icon? icon = null;
        try
        {
            icon = Icon.FromHandle(iconPtr);
            return (Icon)icon.Clone();
        }
        finally
        {
            icon?.Dispose();
            DestroyIcon(iconPtr);
        }
    }

    public static void DestroyIconPointer(this IntPtr iconPtr)
    {
        if (iconPtr == IntPtr.Zero)
        {
            return;
        }

        DestroyIcon(iconPtr);
    }
}