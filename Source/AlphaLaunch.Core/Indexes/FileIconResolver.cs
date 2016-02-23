using System.Drawing;

namespace AlphaLaunch.Core.Indexes
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
            return Icon.ExtractAssociatedIcon(_fullName);
        }

        public string IconKey => _fullName;
    }
}