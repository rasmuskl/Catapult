using Catapult.Core.Indexes;

namespace Catapult.Core.Actions;

public interface IStandaloneAction : IIndexable
{
    void Run();
}