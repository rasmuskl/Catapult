using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaLaunch.Core.Actions
{
    public interface IItemSink<T>
    {
        void RunAction(T item);
    }
}