using System.Drawing;

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