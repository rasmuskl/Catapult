using System;
using System.Collections.Immutable;
using System.Linq;

namespace Catapult.Core.Actions
{
    public class ActionRegistry
    {
        private ImmutableDictionary<Type, ImmutableList<Type>> _actionMappings = ImmutableDictionary.Create<Type, ImmutableList<Type>>(); 

        public void RegisterAction<TAction>()
        {
            var actionType = typeof(TAction);

            var itemSinkTypes = actionType.GetInterfaces()
                .Where(x => x.Name == typeof(IItemSink<>).Name)
                .SelectMany(x => x.GenericTypeArguments)
                .ToImmutableList();

            foreach (var itemSinkType in itemSinkTypes)
            {
                ImmutableList<Type> existingActionTypes;
                if (_actionMappings.TryGetValue(itemSinkType, out existingActionTypes))
                {
                    _actionMappings = _actionMappings.SetItem(itemSinkType, existingActionTypes.Add(actionType));
                }
                else
                {
                    _actionMappings = _actionMappings.SetItem(itemSinkType, ImmutableList.Create(actionType));
                }
            }
        }

        public ImmutableList<Type> GetActionFor(Type itemType)
        {
            ImmutableList<Type> actionTypeList;

            if (_actionMappings.TryGetValue(itemType, out actionTypeList))
            {
                return actionTypeList;
            }

            return ImmutableList.Create<Type>();
        }
    }
}