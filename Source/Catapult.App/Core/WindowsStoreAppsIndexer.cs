using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Windows.Management.Deployment;
using Catapult.Core.Indexes;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Catapult.App.Core
{
    public class WindowsStoreAppsIndexer
    {
        public X[] GetWindowsStoreApps()
        {
            var packageManager = new PackageManager();
            var packages = packageManager.FindPackagesForUser(string.Empty);

            var list = new List<X>();

            foreach (var package in packages)
            {
                var dir = package.InstalledLocation.Path;
                var file = Path.Combine(dir, "AppxManifest.xml");
                var priPath = Path.Combine(dir, "resources.pri");

                var xDocument = XDocument.Load(file);
                var namespaceManager = new XmlNamespaceManager(new NameTable());
                namespaceManager.AddNamespace("x", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
                namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");
                namespaceManager.AddNamespace("uap3", "http://schemas.microsoft.com/appx/manifest/uap/windows10/3");
                namespaceManager.AddNamespace("uap5", "http://schemas.microsoft.com/appx/manifest/uap/windows10/5");
                foreach (var application in xDocument.XPathSelectElements("//x:Applications/x:Application",
                    namespaceManager))
                {
                    var visualElements = application.XPathSelectElement("./uap:VisualElements", namespaceManager);

                    if (visualElements == null)
                    {
                        continue;
                    }

                    //var xPathSelectElement = application.XPathSelectElement(".//uap3:Extension[@Category='windows.appExecutionAlias']", namespaceManager);

                    //if (xPathSelectElement == null)
                    //{
                    //    xPathSelectElement = application.XPathSelectElement(".//uap5:Extension[@Category='windows.appExecutionAlias']", namespaceManager);

                    //    if (xPathSelectElement is null)
                    //    {
                    //        continue;

                    //    }
                    //}

                    var appId = application.Attribute(XName.Get("Id"))?.Value;
                    var appDisplayName = visualElements.Attribute(XName.Get("DisplayName"))?.Value;

                    if (Uri.TryCreate(appDisplayName, UriKind.Absolute, out var uri))
                    {
                        var resource = $"ms-resource://{package.Id.Name}/resources/{uri.Segments.Last()}";
                        appDisplayName = ExtractStringFromPRIFile(priPath, resource);
                        if (string.IsNullOrWhiteSpace(appDisplayName))
                        {

                            var res = string.Concat(uri.Segments.Skip(1));
                            resource = $"ms-resource://{package.Id.Name}/{res}";
                            appDisplayName = ExtractStringFromPRIFile(priPath, resource);
                        }

                        var appUserModelId = $"{package.Id.FamilyName}!{appId}";

                        list.Add(new X { DisplayName = appDisplayName, AppUserModelId = appUserModelId, Manifest = file });
                        //var appActiveManager = new ApplicationActivationManager();
                        //uint pid;
                        //appActiveManager.ActivateApplication(appUserModelId, null, ActivateOptions.None, out pid);
                    }


                    //foreach (var application in obj.Applications)
                    //{
                    //    try
                    //    {
                    //        //itemView.DisplayName = ExtractDisplayName(dir, package, application);
                    //        //itemView.ForegroundText = application.VisualElements.ForegroundText;
                    //        ////itemView.DisplayIcon = ExtractDisplayIcon(dir, application);
                    //        //itemView.DisplayIcon = ExtractDisplayIcon2(dir, application);
                    //        //itemView.IconBackground = application.VisualElements.BackgroundColor;
                    //        //itemView.AppUserModelId = GetAppUserModelId(package.Id.FullName, application.Id);

                    //        //Packages.Add(itemView);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Debug.WriteLine(ex.Message);
                    //    }
                    //}
                }

            }

            return new X[0];
        }

        //private static string ExtractDisplayIcon2(string dir, Application application)
        //{
        //    var logo = Path.Combine(dir, application.VisualElements.SmallLogo);
        //    if (File.Exists(logo))
        //        return logo;

        //    logo = Path.Combine(dir, Path.ChangeExtension(logo, "scale-100.png"));
        //    if (File.Exists(logo))
        //        return logo;

        //    var localized = Path.Combine(dir, "en-us", application.VisualElements.SmallLogo); //TODO: How determine if culture parameter is necessary?
        //    localized = Path.Combine(dir, Path.ChangeExtension(localized, "scale-100.png"));
        //    return localized;
        //}

        //private static string ExtractDisplayIcon(string dir, Application application)
        //{
        //    var imageFile = Path.Combine(dir, application.VisualElements.Logo);
        //    var scaleImage = Path.ChangeExtension(imageFile, "scale-100.png");
        //    if (File.Exists(scaleImage))
        //        return scaleImage;

        //    if (File.Exists(imageFile))
        //        return imageFile;

        //    imageFile = Path.Combine(dir, "en-us", application.VisualElements.Logo); //TODO: How determine if culture parameter is necessary?
        //    if (File.Exists(imageFile))
        //        return imageFile;

        //    return Path.ChangeExtension(imageFile, "scale-100.png");
        //}

        //private static string ExtractDisplayName(string dir, Windows.ApplicationModel.Package package, Application application)
        //{
        //    var priPath = Path.Combine(dir, "resources.pri");
        //    Uri uri;
        //    if (!Uri.TryCreate(application.VisualElements.DisplayName, UriKind.Absolute, out uri))
        //        return application.VisualElements.DisplayName;

        //    var resource = string.Format("ms-resource://{0}/resources/{1}", package.Id.Name, uri.Segments.Last());
        //    var name = ExtractStringFromPRIFile(priPath, resource);
        //    if (!string.IsNullOrWhiteSpace(name))
        //        return name;

        //    var res = string.Concat(uri.Segments.Skip(1));
        //    resource = string.Format("ms-resource://{0}/{1}", package.Id.Name, res);
        //    return ExtractStringFromPRIFile(priPath, resource);
        //}

        static internal string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        {
            string sWin8ManifestString = string.Format("@{{{0}? {1}}}", pathToPRI, resourceKey);
            var outBuff = new StringBuilder(1024);
            int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
            return outBuff.ToString();
        }

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        public enum ActivateOptions
        {
            None = 0x00000000,  // No flags set
            DesignMode = 0x00000001,  // The application is being activated for design mode, and thus will not be able to
            // to create an immersive window. Window creation must be done by design tools which
            // load the necessary components by communicating with a designer-specified service on
            // the site chain established on the activation manager.  The splash screen normally
            // shown when an application is activated will also not appear.  Most activations
            // will not use this flag.
            NoErrorUI = 0x00000002,  // Do not show an error dialog if the app fails to activate.
            NoSplashScreen = 0x00000004,  // Do not show the splash screen when activating the app.
        }

        [ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IApplicationActivationManager
        {
            // Activates the specified immersive application for the "Launch" contract, passing the provided arguments
            // string into the application.  Callers can obtain the process Id of the application instance fulfilling this contract.
            IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
            IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
            IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
        }

        [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]//Application Activation Manager
        public class ApplicationActivationManager : IApplicationActivationManager
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)/*, PreserveSig*/]
            public extern IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            public extern IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            public extern IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
        }
    }

    public class X : IndexableBase
    {


        public override string Name => "X";
        public string DisplayName { get; set; }
        public string AppUserModelId { get; set; }
        public string Manifest { get; set; }

        public override string ToString()
        {
            return $"{nameof(DisplayName)}: {DisplayName}, {nameof(AppUserModelId)}: {AppUserModelId}";
        }
    }
}