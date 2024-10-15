using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public interface IAction
{
}

public interface IAction<in T> : IIndexable, IAction where T : IIndexable
{
    void Run(T parameter);
}