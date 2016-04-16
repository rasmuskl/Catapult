namespace Catapult.Core.Actions
{
    public interface IItemSink<T>
    {
        void RunAction(T item);
    }
}