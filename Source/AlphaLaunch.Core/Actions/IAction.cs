using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public interface IAction<T> : IIndexable
    {
        void RunAction(T parameter);
    }
}