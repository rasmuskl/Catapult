using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public interface IAction<T> : IIndexable
    {
        void RunAction(T parameter);
    }
}