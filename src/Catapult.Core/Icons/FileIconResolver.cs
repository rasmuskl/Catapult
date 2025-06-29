using System.Drawing;

namespace Catapult.Core.Icons;

public class FileIconResolver(string fullName) : IIconResolver
{
    public static Func<string, Icon?> ResolverFunc { get; set; }

    public Icon? Resolve()
    {
        return ResolverFunc(fullName);
    }

    public string IconKey => fullName;
}