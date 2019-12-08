using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using Catapult.Core.Indexes;
using Serilog;

namespace Catapult.App.Core
{
    public class WindowsStoreAppIndexer
    {
        public WindowsStoreAppItem[] GetWindowsStoreApps()
        {
            try
            {
                Collection<StartAppsResult> startAppsResults;
                Collection<GetAppxPackageResult> getAppxPackageResults;

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddCommand("Get-StartApps");
                    startAppsResults = powerShell.Invoke<StartAppsResult>();
                }

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript("Get-AppxPackage | Select PackageFamilyName");
                    getAppxPackageResults = powerShell.Invoke<GetAppxPackageResult>();
                }

                var startAppsSet = startAppsResults.Where(x => x.AppID.Contains("!")).ToDictionary(x => x.AppID.Split('!').First(), x => x);
                var packageResults = (from packageResult in getAppxPackageResults
                        where startAppsSet.ContainsKey(packageResult.PackageFamilyName)
                        let startApp = startAppsSet[packageResult.PackageFamilyName]
                        select new WindowsStoreAppItem { DisplayName = startApp.Name, AppUserModelId = startApp.AppID })
                    .ToArray();

                return packageResults;
            }
            catch (Exception ex)
            {
                Log.Error("Indexing Windows Store Apps failed.", ex);
                return Array.Empty<WindowsStoreAppItem>();
            }
        }
    }

    public class GetAppxPackageResult
    {
        public string PackageFamilyName { get; set; }
    }

    public class StartAppsResult
    {
        public string Name { get; set; }
        public string AppID { get; set; }
    }

    public class WindowsStoreAppItem : IndexableBase
    {
        public override string Name => DisplayName;
        public string DisplayName { get; set; }
        public string AppUserModelId { get; set; }
    }
}