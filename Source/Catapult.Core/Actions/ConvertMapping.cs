using System;

namespace Catapult.Core.Actions
{
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