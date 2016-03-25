using System.Drawing;
using AlphaLaunch.Core.Icons;

namespace AlphaLaunch.Core.Indexes
{
    public class EmptyIconResolver : IIconResolver
    {
        public Icon Resolve()
        {
            return null;
        }

        public string IconKey => null;
    }
}