using Catapult.Core.Indexes;
using Microsoft.Win32;

namespace Catapult.Core.Actions
{
    public class DisableRunAtStartUpAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            rkApp.DeleteValue("Catapult");
        }

        public override string Name => "Catapult: Disable run at Windows start";
    }
}