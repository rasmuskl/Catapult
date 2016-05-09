using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class ActionRegistry
    {
        private ImmutableDictionary<Type, ImmutableList<Type>> _actionMappings = ImmutableDictionary.Create<Type, ImmutableList<Type>>();
        private readonly List<IIndexable> _actions = new List<IIndexable>();

        public void RegisterAction<TAction>() where TAction : IIndexable, new()
        {
            var actionType = typeof(TAction);

            var actionTypes = actionType.GetInterfaces()
                .Where(x => x.Name == typeof(IAction<>).Name)
                .SelectMany(x => x.GenericTypeArguments)
                .ToImmutableList();

            _actions.Add(new TAction());

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


        public ImmutableList<Type> GetTypesFor(Type actionType)
        {
            return _actionMappings.Where(x => x.Value.Contains(actionType))
                .Select(x => x.Key)
                .ToImmutableList();
        }

        public ISearchFrame GetSearchFrame(Type type)
        {
            if (type == null)
            {
                return new IndexableSearchFrame(SearchResources.GetFiles().Concat(_actions).ToArray());
            }

            var closedActionType = GetInstanceOfGenericType(typeof(IAction<>), type);

            if (closedActionType != null && closedActionType == typeof(IAction<StringIndexable>))
            {
                return new StringSearchFrame(null);
            }

            var actionTypes = GetActionFor(type);

            if (actionTypes.Any())
            {
                var indexables = actionTypes.Select(Activator.CreateInstance).OfType<IIndexable>().ToArray();
                return new IndexableSearchFrame(indexables);
            }

            return new IndexableSearchFrame(SearchResources.GetFiles().Concat(_actions).ToArray());
        }

        static Type GetInstanceOfGenericType(Type genericType, Type instanceType)
        {
            while (instanceType != null)
            {
                if (instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == genericType)
                {
                    return instanceType;
                }

                foreach (var i in instanceType.GetInterfaces())
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                    {
                        return i;
                    }
                }

                instanceType = instanceType.BaseType;
            }

            return null;
        }
    }
}