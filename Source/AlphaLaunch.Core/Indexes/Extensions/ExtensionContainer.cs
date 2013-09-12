using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AlphaLaunch.Core.Indexes.Extensions
{
    public class ExtensionContainer
    {
        private readonly ImmutableDictionary<string, ExtensionInfo> _extensionInfos;

        public ExtensionContainer(IEnumerable<ExtensionInfo> extensionInfos)
        {
            _extensionInfos = extensionInfos.ToImmutableDictionary(x => x.Extension, x => x);
        }

        public bool IsKnownExtension(string extension)
        {
            return _extensionInfos.ContainsKey(extension);
        }
    }
}