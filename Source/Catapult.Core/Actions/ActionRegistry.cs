using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class ActionRegistry
    {
        private ImmutableList<ActionMapping> _actionMappings = ImmutableList.Create<ActionMapping>();
        private ImmutableList<ConvertMapping> _convertMappings = ImmutableList.Create<ConvertMapping>();
        private readonly List<IIndexable> _actions = new List<IIndexable>();

        public void RegisterAction<T>() where T : IIndexable, new()
        {
            _actions.Add(new T());

            var actionType = typeof(T);

            ImmutableList<ActionMapping> actionTypesOne = actionType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAction<>))
                .Select(x => new ActionMapping(actionType, x.GenericTypeArguments[0]))
                .ToImmutableList();

            _actionMappings = _actionMappings.AddRange(actionTypesOne);

            ImmutableList<ConvertMapping> convertTypes = actionType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConvert<,>))
                .Select(x => new ConvertMapping(actionType, x.GenericTypeArguments[0], x.GenericTypeArguments[1]))
                .ToImmutableList();

            _convertMappings = _convertMappings.AddRange(convertTypes);
        }

        public ImmutableList<ActionMapping> GetActionForInType(Type itemType)
        {
            return _actionMappings
                .Where(x => x.InType == itemType)
                .ToImmutableList();
        }

        public ImmutableList<ConvertMapping> GetConvertForInType(Type itemType)
        {
            return _convertMappings
                .Where(x => x.InType == itemType)
                .ToImmutableList();
        }

        public ImmutableList<ActionMapping> GetTypesFor(Type actionType)
        {
            return _actionMappings
                .Where(x => x.ActionType == actionType)
                .ToImmutableList();
        }

        public ISearchFrame GetSearchFrame(IIndexable[] indexables)
        {
            indexables = indexables?.Where(x => x != null).ToArray() ?? new IIndexable[0];

            if (!indexables.Any())
            {
                return new IndexableSearchFrame(SearchResources.GetFiles().Concat(_actions).Concat(new ControlPanelIndexer().GetControlPanelItems()).ToArray());
            }

            Type type = indexables.First().GetType();

            var closedActionType = GetInstanceOfGenericType(typeof(IAction<>), type);

            if (closedActionType != null && closedActionType == typeof(IAction<StringIndexable>))
            {
                if (typeof(IAutocomplete).IsAssignableFrom(type))
                {
                    IAutocomplete autocomplete = (IAutocomplete)Activator.CreateInstance(type);
                    return new StringSearchFrame(autocomplete.GetAutocompleteResults);
                }

                return new StringSearchFrame(null);
            }

            var targetTypes = new Type[0];

            foreach (IIndexable indexable in indexables)
            {
                if (indexable is IAction)
                {
                    if (targetTypes.Any())
                    {
                        return null;
                    }

                    var actionParameterTypes = indexable.GetType().GetInterfaces()
                        .Where(x => x.IsGenericTypeDefinition && x.GetGenericTypeDefinition() == typeof(IAction<>))
                        .Select(x => x.GenericTypeArguments[0])
                        .ToArray();

                    targetTypes = actionParameterTypes;
                }
                else if (indexable is IConvert)
                {
                    if (targetTypes.Length == 1)
                    {
                        var convertParameterTypes = indexable.GetType().GetInterfaces()
                           .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConvert<,>) && x.GenericTypeArguments[0] == targetTypes[0])
                           .Select(x => x.GenericTypeArguments[1])
                           .ToArray();

                        targetTypes = convertParameterTypes;
                    }
                }
                else
                {
                    targetTypes = new[] { indexable.GetType() };
                }
            }

            var actionTypes = targetTypes
                .SelectMany(x => GetActionForInType(x).Select(y => y.ActionType))
                .ToArray();

            var convertTypes = targetTypes
                .SelectMany(x => GetConvertForInType(x).Select(y => y.ConvertType))
                .ToArray();

            if (actionTypes.Any())
            {
                var matchedIndexables = actionTypes.Concat(convertTypes).Select(Activator.CreateInstance).OfType<IIndexable>().ToArray();
                return new IndexableSearchFrame(matchedIndexables);
            }

            return null;
        }

        public Launchable Launch(IIndexable[] indexables)
        {
            IIndexable action = null;
            IIndexable target = null;

            foreach (IIndexable indexable in indexables)
            {
                if (indexable is IAction)
                {
                    action = indexable;
                    continue;
                }
                else if (indexable is IConvert)
                {
                    if (target == null)
                    {
                        throw new NotSupportedException("Converts before target are not supported yet.");
                    }

                    var convertParameterTypes = indexable.GetType().GetInterfaces()
                        .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConvert<,>) && x.GenericTypeArguments[0] == target.GetType())
                        .Select(x => x.GenericTypeArguments[1])
                        .ToArray();

                    if (convertParameterTypes.Length != 1)
                    {
                        throw new InvalidOperationException("Unexpected convert parameter type count: " +
                                                            convertParameterTypes.Length);
                    }

                    target = (IIndexable)indexable.GetType()
                        .GetMethod("Convert", new[] { target.GetType() })
                        .Invoke(indexable, new[] { target });
                }
                else
                {
                    target = indexable;
                }
            }

            if (target != null && action == null)
            {
                var actionList = GetActionForInType(target.GetType());
                action = (IIndexable)Activator.CreateInstance(actionList.First().ActionType);
            }

            return new Launchable(action, target);
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

    public class Launchable
    {
        public IIndexable Action { get; set; }
        public IIndexable Target { get; set; }

        public Launchable(IIndexable action, IIndexable target)
        {
            Action = action;
            Target = target;
        }
    }

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

    public class ConvertMapping
    {
        public ConvertMapping(Type convertType, Type inType, Type outType)
        {
            ConvertType = convertType;
            InType = inType;
            OutType = outType;
        }

        public Type ConvertType { get; }
        public Type InType { get; }
        public Type OutType { get; }
    }
}