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

            var actionTypes = actionType.GetInterfaces()
                .Where(x => x.Name == typeof(IAction<>).Name)
                .SelectMany(x => x.GenericTypeArguments)
                .ToImmutableList();

            foreach (var type in actionTypes)
            {
                ImmutableList<Type> existingActionTypes;
                if (_actionMappings.TryGetValue(type, out existingActionTypes))
                {
                    _actionMappings = _actionMappings.SetItem(type, existingActionTypes.Add(actionType));
                }
                else
                {
                    _actionMappings = _actionMappings.SetItem(type, ImmutableList.Create(actionType));
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