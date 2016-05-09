using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public interface IConvert
    {
        
    }

    public interface IConvert<in T, out T2> : IIndexable, IConvert where T : IIndexable where T2 : IIndexable
    {
        T2 Convert(T item);
    }
}