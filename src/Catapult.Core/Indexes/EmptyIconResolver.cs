using System.Drawing;
using Catapult.Core.Icons;

namespace Catapult.Core.Indexes;

public class EmptyIconResolver : IIconResolver
{
    public Icon? Resolve()
    {
        return null;
    }

    public string? IconKey => null;
}