using System.Drawing;

namespace Catapult.Core.Icons;

public interface IIconResolver
{
    Icon? Resolve();
    string? IconKey { get; }
}