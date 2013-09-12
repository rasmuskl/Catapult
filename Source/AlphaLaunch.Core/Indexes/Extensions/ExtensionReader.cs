using System.Collections.Generic;
using Microsoft.Win32;

namespace AlphaLaunch.Core.Indexes.Extensions
{
    public class ExtensionReader
    {
        public ExtensionContainer ReadRegistry()
        {
            var subKeyNames = Registry.ClassesRoot.GetSubKeyNames();

            var extensions = new List<ExtensionInfo>();

            foreach (var subKeyName in subKeyNames)
            {
                if (!subKeyName.StartsWith("."))
                {
                    continue;
                }

                var extensionKey = Registry.ClassesRoot.OpenSubKey(subKeyName);

                if (extensionKey == null)
                {
                    continue;
                }

                var handlerName = extensionKey.GetValue("") as string;

                if (handlerName == null)
                {
                    continue;
                }

                var handlerSubKey = Registry.ClassesRoot.OpenSubKey(handlerName);

                if (handlerSubKey == null)
                {
                    continue;
                }

                if (handlerSubKey.GetValue("NoOpen") != null)
                {
                    continue;
                }

                extensions.Add(new ExtensionInfo(subKeyName));
            }

            return new ExtensionContainer(extensions);
        }
    }
}