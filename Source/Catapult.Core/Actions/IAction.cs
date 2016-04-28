using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public interface IAction<T> : IIndexable, IAction
    {
        void RunAction(T parameter);
    }
}