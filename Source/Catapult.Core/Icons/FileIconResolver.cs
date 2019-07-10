using System;
using System.Drawing;

namespace Catapult.Core.Icons
{
    public class FileIconResolver : IIconResolver
    {
        private readonly string _fullName;

        public static Func<string, Icon> ResolverFunc { get; set; }

        public FileIconResolver(string fullName)
        {
            _fullName = fullName;
        }

        public Icon Resolve()
        {
            return ResolverFunc(_fullName);
        }

        public string IconKey => _fullName;
    }
}