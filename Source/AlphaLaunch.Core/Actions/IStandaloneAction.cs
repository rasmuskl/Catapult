using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public interface IStandaloneAction : IIndexable
    {
        void RunAction();
    }
}