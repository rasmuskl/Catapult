using System.Drawing;

namespace AlphaLaunch.Core.Icons
{
    public interface IIconResolver
    {
        Icon Resolve();
        string IconKey { get; }
    }
}