using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaLaunch.Core.Icons
{
    public class FileIconResolver : IIconResolver
    {
        private readonly string _fullName;

        public FileIconResolver(string fullName)
        {
            _fullName = fullName;
        }

        public Icon Resolve()
        {
            var icon = ShellIcons.GetIcon(_fullName);

            if (icon != null)
            {
                return icon;
            }

            return Icon.ExtractAssociatedIcon(_fullName);
        }

        public string IconKey => _fullName;
    }

    public static class ShellIcons
    {
        public static Icon GetIcon(string path)
        {
            var directoryName = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            IntPtr folderPtr;
            if (SHGetDesktopFolder(out folderPtr) != SOk)
            {
                return null;
            }

            var desktopFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(folderPtr, typeof(IShellFolder));
            try
            {
                Sfgao pdwAttributes = 0;
                uint pchEaten = 0;
                IntPtr ppidl;
                desktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, directoryName, ref pchEaten, out ppidl, ref pdwAttributes);

                Guid iidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
                IntPtr ppv;
                desktopFolder.BindToObject(ppidl, IntPtr.Zero, iidShellFolder, out ppv);

                var folder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(ppv, typeof(IShellFolder));

                try
                {
                    folder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, fileName, ref pchEaten, out ppidl, ref pdwAttributes);

                    try
                    {
                        return ExtractIcon(folder, ppidl);
                    }
                    finally
                    {
                        Marshal.Release(ppidl);
                    }
                }
                finally
                {
                    Marshal.Release(ppv);
                    Marshal.ReleaseComObject(folder);
                }
            }
            finally
            {
                Marshal.Release(folderPtr);
                Marshal.ReleaseComObject(desktopFolder);
            }
        }

        [DllImport("shell32.dll")]
        public static extern Int32 SHGetDesktopFolder(out IntPtr ppshf);

        private const int SOk = 0;
        private const int MaxPath = 260;

        private static Icon ExtractIcon(IShellFolder parentFolder, IntPtr pidl)
        {
            Guid extractIconGuid = typeof(IExtractIcon).GUID;
            IntPtr extractIconPtr;

            if (parentFolder.GetUIObjectOf(IntPtr.Zero, 1, new[] { pidl }, ref extractIconGuid, IntPtr.Zero, out extractIconPtr) != SOk)
            {
                return null;
            }

            var iconExtractor = (IExtractIcon)Marshal.GetTypedObjectForIUnknown(extractIconPtr, typeof(IExtractIcon));

            try
            {
                var location = new StringBuilder(MaxPath, MaxPath);
                int iconIndex;
                var flags = ExtractIconuFlags.GilForshell;
                ExtractIconpwFlags pwFlags;

                if (iconExtractor.GetIconLocation(flags, location, location.Capacity, out iconIndex, out pwFlags) != SOk)
                {
                    return null;
                }

                string path = location.ToString();
                IntPtr largeIconHandle;
                IntPtr smallIconHandle;
                uint iconSize = 32 + (16 << 16);

                if (iconExtractor.Extract(path, (uint)iconIndex, out largeIconHandle, out smallIconHandle, iconSize) != SOk)
                {
                    return null;
                }

                try
                {
                    return Icon.FromHandle(largeIconHandle);
                }
                finally
                {
                    Marshal.Release(largeIconHandle);
                    Marshal.Release(smallIconHandle);
                }
            }
            finally
            {
                Marshal.Release(extractIconPtr);
                Marshal.ReleaseComObject(iconExtractor);
            }
        }
    }

    [ComImport()]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214fa-0000-0000-c000-000000000046")]
    public interface IExtractIcon
    {
        [PreserveSig]
        int GetIconLocation(ExtractIconuFlags uFlags,
            [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder szIconFile,
            int cchMax,
            out int piIndex,
            out ExtractIconpwFlags pwFlags);

        [PreserveSig]
        int Extract(string pszFile,
            uint nIconIndex,
            out IntPtr phiconLarge,
            out IntPtr phiconSmall,
            uint nIconSize);
    }

    [ComImport()]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        [PreserveSig]
        Int32 ParseDisplayName(
            IntPtr hwnd,
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPWStr)]
                string pszDisplayName,
            ref uint pchEaten,
            out IntPtr ppidl,
            ref Sfgao pdwAttributes);

        [PreserveSig]
        Int32 EnumObjects(
            IntPtr hwnd,
            Shcontf grfFlags,
            out IntPtr enumIdList);

        [PreserveSig]
        Int32 BindToObject(
            IntPtr pidl,
            IntPtr pbc,
            ref Guid riid,
            out IntPtr ppv);

        [PreserveSig]
        Int32 BindToStorage(
            IntPtr pidl,
            IntPtr pbc,
            ref Guid riid,
            out IntPtr ppv);

        [PreserveSig]
        Int32 CompareIDs(
            IntPtr lParam,
            IntPtr pidl1,
            IntPtr pidl2);

        [PreserveSig]
        Int32 CreateViewObject(
            IntPtr hwndOwner,
            Guid riid,
            out IntPtr ppv);

        [PreserveSig]
        Int32 GetAttributesOf(
            uint cidl,
            [MarshalAs(UnmanagedType.LPArray)]
                IntPtr[] apidl,
            ref Sfgao rgfInOut);

        [PreserveSig]
        Int32 GetUIObjectOf(
            IntPtr hwndOwner,
            uint cidl,
            [MarshalAs(UnmanagedType.LPArray)]
                IntPtr[] apidl,
            ref Guid riid,
            IntPtr rgfReserved,
            out IntPtr ppv);

        [PreserveSig()]
        Int32 GetDisplayNameOf(
            IntPtr pidl,
            Shgno uFlags,
            IntPtr lpName);

        [PreserveSig]
        Int32 SetNameOf(
            IntPtr hwnd,
            IntPtr pidl,
            [MarshalAs(UnmanagedType.LPWStr)]
                string pszName,
            Shgno uFlags,
            out IntPtr ppidlOut);
    }

    [ComImport()]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F2-0000-0000-C000-000000000046")]
    public interface IEnumIDList
    {
        // Retrieves the specified number of item identifiers in the enumeration 
        // sequence and advances the current position by the number of items retrieved
        [PreserveSig()]
        Int32 Next(
            int celt,
            out IntPtr rgelt,
            out int pceltFetched);

        // Skips over the specified number of elements in the enumeration sequence
        [PreserveSig()]
        Int32 Skip(
            int celt);

        // Returns to the beginning of the enumeration sequence
        [PreserveSig()]
        Int32 Reset();

        // Creates a new item enumeration object with the same contents and state as the current one
        [PreserveSig()]
        Int32 Clone(
            out IEnumIDList ppenum);
    }

    [Flags()]
    public enum ExtractIconuFlags : uint
    {
        GilAsync = 0x0020,
        GilDefaulticon = 0x0040,
        GilForshell = 0x0002,
        GilForshortcut = 0x0080,
        GilOpenicon = 0x0001,
        GilCheckshield = 0x0200
    }

    [Flags()]
    public enum ExtractIconpwFlags : uint
    {
        GilDontcache = 0x0010,
        GilNotfilename = 0x0008,
        GilPerclass = 0x0004,
        GilPerinstance = 0x0002,
        GilSimulatedoc = 0x0001,
        GilShield = 0x0200,
        GilForcenoshield = 0x0400
    }


    [Flags()]
    public enum Shcontf : uint
    {
        Folders = 0x0020,
        Nonfolders = 0x0040,
        Includehidden = 0x0080,
        InitOnFirstNext = 0x0100,
        Netprintersrch = 0x0200,
        Shareable = 0x0400,
        Storage = 0x0800,
    }

    [Flags()]
    public enum Sfgao : uint
    {
        Browsable = 0x8000000,
        Cancopy = 1,
        Candelete = 0x20,
        Canlink = 4,
        Canmoniker = 0x400000,
        Canmove = 2,
        Canrename = 0x10,
        Capabilitymask = 0x177,
        Compressed = 0x4000000,
        Contentsmask = 0x80000000,
        Displayattrmask = 0xfc000,
        Droptarget = 0x100,
        Encrypted = 0x2000,
        Filesysancestor = 0x10000000,
        Filesystem = 0x40000000,
        Folder = 0x20000000,
        Ghosted = 0x8000,
        Haspropsheet = 0x40,
        Hasstorage = 0x400000,
        Hassubfolder = 0x80000000,
        Hidden = 0x80000,
        Isslow = 0x4000,
        Link = 0x10000,
        Newcontent = 0x200000,
        Nonenumerated = 0x100000,
        Readonly = 0x40000,
        Removable = 0x2000000,
        Share = 0x20000,
        Storage = 8,
        Storageancestor = 0x800000,
        Storagecapmask = 0x70c50008,
        Stream = 0x400000,
        Validate = 0x1000000
    }

    [Flags()]
    public enum Shgno : uint
    {
        Normal = 0x0000,
        Infolder = 0x0001,
        Forediting = 0x1000,
        Foraddressbar = 0x4000,
        Forparsing = 0x8000
    }

}