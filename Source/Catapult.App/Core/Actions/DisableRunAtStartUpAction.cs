using Catapult.Core.Indexes;
using Microsoft.Win32;

namespace Catapult.Core.Actions;

public class DisableRunAtStartUpAction : IndexableBase, IStandaloneAction
{
    public void Run()
    {
        RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        runKey?.DeleteValue(EnableRunAtStartUpAction.CatapultKey);
    }

    public static void RemoveRunAtStartUp()
    {
        RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (runKey?.GetValue(EnableRunAtStartUpAction.CatapultKey) != null)
        {
            runKey.DeleteValue(EnableRunAtStartUpAction.CatapultKey);
        }
    }

    public override string Name => "Catapult: Disable run at Windows start";
}