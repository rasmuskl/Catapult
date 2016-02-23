using System.Drawing;

namespace AlphaLaunch.Core.Indexes
{
    public interface IIconResolver
    {
        Icon Resolve();
        string IconKey { get; }
    }
}