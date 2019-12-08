using System;
using System.Linq;
using Catapult.Core.Indexes;
using Microsoft.WindowsAPICodePack.Shell;
using Serilog;

namespace Catapult.App.Core
{
    public class WindowsStoreAppIndexer
    {
        public WindowsStoreAppItem[] GetWindowsStoreApps()
        {
            try
            {
                Log.Information("Starting Windows Store indexer");

                // GUID taken from https://docs.microsoft.com/en-us/windows/win32/shell/knownfolderid
                var FODLERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
                var appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);

                var startApps = ((IKnownFolder)appsFolder).Select(x => new { x.Name, AppUserModelId = x.Properties.System.AppUserModel.ID.Value }).ToArray();
                return startApps.Where(x => x.AppUserModelId.Contains("!")).Select(x => new WindowsStoreAppItem {DisplayName = x.Name, AppUserModelId = x.AppUserModelId}).ToArray();

                //var packageManager = new PackageManager();
                //var packages = packageManager.FindPackagesForUser(string.Empty);

                //foreach (var package in packages.Reverse())
                //{
                //    var packageIdName = package.Id.Name;

                //    if (!startApps.Any(x => x.AppUserModelId.StartsWith(packageIdName)))
                //    {
                //        continue;
                //    }

                //    var dir = package.InstalledLocation.Path;
                //    var file = Path.Combine(dir, "AppxManifest.xml");
                //    var priPath = Path.Combine(dir, "resources.pri");

                //    var xDocument = XDocument.Load(file);
                //    var namespaceManager = new XmlNamespaceManager(new NameTable());
                //    namespaceManager.AddNamespace("x", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
                //    namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");
                //    foreach (var application in xDocument.XPathSelectElements("//x:Applications/x:Application", namespaceManager))
                //    {
                //        var visualElements = application.XPathSelectElement("./uap:VisualElements", namespaceManager);

                //        if (visualElements == null)
                //        {
                //            continue;
                //        }

                //        var appId = application.Attribute(XName.Get("Id"))?.Value;
                //        var appDisplayName = visualElements.Attribute(XName.Get("DisplayName"))?.Value;

                //        if (Uri.TryCreate(appDisplayName, UriKind.Absolute, out var uri))
                //        {
                //            var resource = $"ms-resource://{packageIdName}/resources/{uri.Segments.Last()}";
                //            appDisplayName = ExtractStringFromPRIFile(priPath, resource);
                //            if (string.IsNullOrWhiteSpace(appDisplayName))
                //            {

                //                var res = string.Concat(uri.Segments.Skip(1));
                //                resource = $"ms-resource://{packageIdName}/{res}";
                //                appDisplayName = ExtractStringFromPRIFile(priPath, resource);
                //            }

                //            var appUserModelId = $"{package.Id.FamilyName}!{appId}";

                //            var startApp = startApps.FirstOrDefault(x => x.AppUserModelId == appUserModelId);

                //            if (startApp != null)
                //            {

                //            }
                //        }
                //    }
                //}


                //Log.Information($"Indexed {list.Count} Windows Store Apps");
                //return list.ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Indexing Windows Store Apps failed.");
                return Array.Empty<WindowsStoreAppItem>();
            }
        }

        //static internal string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        //{
        //    string sWin8ManifestString = string.Format("@{{{0}? {1}}}", pathToPRI, resourceKey);
        //    var outBuff = new StringBuilder(1024);
        //    int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
        //    return outBuff.ToString();
        //}

        //[DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        //private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

    }

    public class WindowsStoreAppItem : IndexableBase
    {
        public override string Name => DisplayName;
        public string DisplayName { get; set; }
        public string AppUserModelId { get; set; }
    }
}