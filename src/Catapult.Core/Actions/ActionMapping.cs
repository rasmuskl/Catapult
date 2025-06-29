namespace Catapult.Core.Actions;

public class ActionMapping(Type actionType, Type inType)
{
    public Type ActionType { get; } = actionType;
    public Type InType { get; } = inType;
}