using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Catapult.Core.Icons
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
            if (".url".Equals(Path.GetExtension(_fullName), StringComparison.InvariantCultureIgnoreCase))
            {
                var lines = File.ReadAllLines(_fullName);

                string iconPath = null;
                int iconIndex = 0;

                foreach (var line in lines)
                {
                    if (line.StartsWith("IconIndex="))
                    {
                        iconIndex = int.Parse(line.Substring("IconIndex=".Length));
                    }
                    else if (line.StartsWith("IconFile="))
                    {
                        iconPath = line.Substring("IconFile=".Length);
                    }
                }

                if (iconPath.IsSet())
                {
                    return Icons.ExtractOne(iconPath, iconIndex, Icons.SystemIconSize.Large);
                }
            }

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

            var desktopFolder = (IShellFolder) Marshal.GetTypedObjectForIUnknown(folderPtr, typeof(IShellFolder));
            try
            {
                Sfgao pdwAttributes = 0;
                uint pchEaten = 0;
                IntPtr ppidl;
                desktopFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, directoryName, ref pchEaten, out ppidl, ref pdwAttributes);

                Guid iidShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
                IntPtr ppv;
                desktopFolder.BindToObject(ppidl, IntPtr.Zero, iidShellFolder, out ppv);

                var folder = (IShellFolder) Marshal.GetTypedObjectForIUnknown(ppv, typeof(IShellFolder));

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
        public static extern int SHGetDesktopFolder(out IntPtr ppshf);

        private const int SOk = 0;
        private const int MaxPath = 260;

        private static Icon ExtractIcon(IShellFolder parentFolder, IntPtr pidl)
        {
            Guid extractIconGuid = typeof(IExtractIcon).GUID;
            IntPtr extractIconPtr;

            if (
                parentFolder.GetUIObjectOf(IntPtr.Zero, 1, new[] {pidl}, ref extractIconGuid, IntPtr.Zero,
                    out extractIconPtr) != SOk)
            {
                return null;
            }

            var iconExtractor = (IExtractIcon) Marshal.GetTypedObjectForIUnknown(extractIconPtr, typeof(IExtractIcon));

            try
            {
                var location = new StringBuilder(MaxPath, MaxPath);
                int iconIndex;
                var flags = ExtractIconuFlags.GIL_FORSHELL;
                ExtractIconpwFlags pwFlags;

                if (iconExtractor.GetIconLocation(flags, location, location.Capacity, out iconIndex, out pwFlags) != SOk)
                {
                    return null;
                }

                string path = location.ToString();
                IntPtr largeIconHandle;
                IntPtr smallIconHandle;
                uint iconSize = 32 + (16 << 16);

                if (iconExtractor.Extract(path, (uint) iconIndex, out largeIconHandle, out smallIconHandle, iconSize) != SOk)
                {
                    return null;
                }

                smallIconHandle.DestroyIconPointer();
                return largeIconHandle.ExtractIconAndDestroyIconPointer();
            }
            finally
            {
                Marshal.Release(extractIconPtr);
                Marshal.ReleaseComObject(iconExtractor);
            }
        }
    }

    [ComImport]
    [Guid("000214fa-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //http://msdn.microsoft.com/en-us/library/windows/desktop/bb761852(v=vs.85).aspx
    interface IExtractIcon
    {
        /// <summary>
        /// Gets the location and index of an icon.
        /// </summary>
        /// <param name="uFlags">One or more of the following values. This parameter can also be NULL.use GIL_ Consts</param>
        /// <param name="szIconFile">A pointer to a buffer that receives the icon location. The icon location is a null-terminated string that identifies the file that contains the icon.</param>
        /// <param name="cchMax">The size of the buffer, in characters, pointed to by pszIconFile.</param>
        /// <param name="piIndex">A pointer to an int that receives the index of the icon in the file pointed to by pszIconFile.</param>
        /// <param name="pwFlags">A pointer to a UINT value that receives zero or a combination of the following value</param>
        [PreserveSig]
        int GetIconLocation(ExtractIconuFlags uFlags, [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder szIconFile, int cchMax, out int piIndex, out ExtractIconpwFlags pwFlags);

        /// <summary>
        /// Extracts an icon image from the specified location.
        /// </summary>
        /// <param name="pszFile">A pointer to a null-terminated string that specifies the icon location.</param>
        /// <param name="nIconIndex">The index of the icon in the file pointed to by pszFile.</param>
        /// <param name="phiconLarge">A pointer to an HICON value that receives the handle to the large icon. This parameter may be NULL.</param>
        /// <param name="phiconSmall">A pointer to an HICON value that receives the handle to the small icon. This parameter may be NULL.</param>
        /// <param name="nIconSize">The desired size of the icon, in pixels. The low word contains the size of the large icon, and the high word contains the size of the small icon. The size specified can be the width or height. The width of an icon always equals its height.</param>
        /// <returns>Returns S_OK if the function extracted the icon, or S_FALSE if the calling application should extract the icon.</returns>
        [PreserveSig]
        int Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile, uint nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);
    }

    [Flags]
    public enum ExtractIconuFlags : uint
    {
        GIL_ASYNC = 0x0020,
        GIL_DEFAULTICON = 0x0040,
        GIL_FORSHELL = 0x0002,
        GIL_FORSHORTCUT = 0x0080,
        GIL_OPENICON = 0x0001,
        GIL_CHECKSHIELD = 0x0200
    }

    [Flags]
    public enum ExtractIconpwFlags : uint
    {
        GIL_DONTCACHE = 0x0010,
        GIL_NOTFILENAME = 0x0008,
        GIL_PERCLASS = 0x0004,
        GIL_PERINSTANCE = 0x0002,
        GIL_SIMULATEDOC = 0x0001,
        GIL_SHIELD = 0x0200, //Windows Vista only
        GIL_FORCENOSHIELD = 0x0400 //Windows Vista only
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        [PreserveSig]
        int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, ref uint pchEaten, out IntPtr ppidl, ref Sfgao pdwAttributes);

        [PreserveSig]
        int EnumObjects(IntPtr hwnd, Shcontf grfFlags, out IntPtr enumIdList);

        [PreserveSig]
        int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

        [PreserveSig]
        int CreateViewObject(IntPtr hwndOwner, Guid riid, out IntPtr ppv);

        [PreserveSig]
        int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref Sfgao rgfInOut);

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);

        [PreserveSig()]
        int GetDisplayNameOf(IntPtr pidl, Shgno uFlags, IntPtr lpName);

        [PreserveSig]
        int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, Shgno uFlags, out IntPtr ppidlOut);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F2-0000-0000-C000-000000000046")]
    public interface IEnumIDList
    {
        // Retrieves the specified number of item identifiers in the enumeration 
        // sequence and advances the current position by the number of items retrieved
        [PreserveSig]
        int Next(int celt, out IntPtr rgelt, out int pceltFetched);

        // Skips over the specified number of elements in the enumeration sequence
        [PreserveSig]
        int Skip(int celt);

        // Returns to the beginning of the enumeration sequence
        [PreserveSig]
        int Reset();

        // Creates a new item enumeration object with the same contents and state as the current one
        [PreserveSig]
        int Clone(out IEnumIDList ppenum);
    }


    [Flags]
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

    [Flags]
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

    [Flags]
    public enum Shgno : uint
    {
        Normal = 0x0000,
        Infolder = 0x0001,
        Forediting = 0x1000,
        Foraddressbar = 0x4000,
        Forparsing = 0x8000
    }

    public static class Icons
    {
        public class IconNotFoundException : Exception
        {
            public IconNotFoundException(string fileName, int index) : base($"Icon with Id = {index} wasn't found in file {fileName}")
            {
            }
        }

        public class UnableToExtractIconsException : Exception
        {
            public UnableToExtractIconsException(string fileName, int firstIconIndex, int iconCount) : base(string.Format("Tried to extract {2} icons starting from the one with id {1} from the \"{0}\" file but failed", fileName, firstIconIndex, iconCount))
            {
            }
        }

        [DllImport("Shell32", CharSet = CharSet.Auto)]
        static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)] string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);

        public enum SystemIconSize
        {
            Large = 0x000000000,
            Small = 0x000000001
        }

        public static Icon ExtractOne(string fileName, int index, SystemIconSize size)
        {
            try
            {
                List<Icon> iconList = ExtractEx(fileName, size, index, 1);
                return iconList[0];
            }
            catch (UnableToExtractIconsException)
            {
                throw new IconNotFoundException(fileName, index);
            }
        }

        private static void ExtractEx(string fileName, List<Icon> largeIcons, List<Icon> smallIcons, int firstIconIndex, int iconCount)
        {
            IntPtr[] smallIconsPtrs = null;
            IntPtr[] largeIconsPtrs = null;

            if (smallIcons != null)
            {
                smallIconsPtrs = new IntPtr[iconCount];
            }
            if (largeIcons != null)
            {
                largeIconsPtrs = new IntPtr[iconCount];
            }

            int apiResult = ExtractIconEx(fileName, firstIconIndex, largeIconsPtrs, smallIconsPtrs, iconCount);
            if (apiResult != iconCount)
            {
                throw new UnableToExtractIconsException(fileName, firstIconIndex, iconCount);
            }

            smallIcons?.AddRange(smallIconsPtrs.Select(x => x.ExtractIconAndDestroyIconPointer()));
            largeIcons?.AddRange(largeIconsPtrs.Select(x => x.ExtractIconAndDestroyIconPointer()));
        }

        private static List<Icon> ExtractEx(string fileName, SystemIconSize size, int firstIconIndex, int iconCount)
        {
            List<Icon> iconList = new List<Icon>();

            switch (size)
            {
                case SystemIconSize.Large:
                    ExtractEx(fileName, iconList, null, firstIconIndex, iconCount);
                    break;

                case SystemIconSize.Small:
                    ExtractEx(fileName, null, iconList, firstIconIndex, iconCount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(size));
            }

            return iconList;
        }
    }

    public static class IntPtrExtensions
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);


        public static Icon ExtractIconAndDestroyIconPointer(this IntPtr iconPtr)
        {
            if (iconPtr == IntPtr.Zero)
            {
                return null;
            }

            Icon icon = null;
            try
            {
                icon = Icon.FromHandle(iconPtr);
                return (Icon) icon.Clone();
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
}