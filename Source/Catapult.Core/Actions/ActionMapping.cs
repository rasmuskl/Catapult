using System;

namespace Catapult.Core.Actions
{
    public class ActionMapping
    {
        public ActionMapping(Type actionType, Type inType)
        {
            ActionType = actionType;
            InType = inType;
        }

        public Type ActionType { get; }
        public Type InType { get; }
    }
}