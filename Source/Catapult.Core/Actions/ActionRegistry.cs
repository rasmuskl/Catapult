using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Catapult.Core.Indexes;
using Catapult.Core.Selecta;

namespace Catapult.Core.Actions
{
    public class ActionRegistry
    {
        private ImmutableList<ActionMapping> _actionMappings = ImmutableList.Create<ActionMapping>();
        private ImmutableList<ConvertMapping> _convertMappings = ImmutableList.Create<ConvertMapping>();
        private readonly List<IIndexable> _actions = new List<IIndexable>();
        private ImmutableList<Func<IEnumerable<IIndexable>>> _indexers;
        private int _updateCounter = 0;


        public ActionRegistry()
        {
            _indexers = ImmutableList.Create<Func<IEnumerable<IIndexable>>>(
                SearchResources.GetFiles,
                () => _actions,
                () => new ChromeBookmarksIndexer().GetBookmarkItems()
            );
        }

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

            _updateCounter += 1;
        }

        public void RegisterIndexer(Func<IEnumerable<IIndexable>> indexerFunc)
        {
            _indexers = _indexers.Add(indexerFunc);
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
                Func<string> getUpdatedState = () => $"{_updateCounter}-{SearchResources.UpdateCounter}";
                Func<IndexableResult> fetchIndexables = () => new IndexableResult(_indexers.SelectMany(x => x()).ToArray(), getUpdatedState());
                var indexableUpdateState = new IndexableUpdateState(fetchIndexables, getUpdatedState);
                return new UpdateableIndexableSearchFrame(indexableUpdateState);
            }

            Type type = indexables.First().GetType();

            var closedActionType = GetInstanceOfGenericType(typeof(IAction<>), type);

            if (closedActionType != null && closedActionType == typeof(IAction<StringIndexable>))
            {
                if (indexables.Length > 1)
                {
                    return null;
                }

                if (typeof(IHasSearchFrame).IsAssignableFrom(type))
                {
                    IHasSearchFrame hasSearchFrame = (IHasSearchFrame)Activator.CreateInstance(type);
                    return hasSearchFrame.GetSearchFrame();
                }

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

                if (indexable is IConvert)
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
}