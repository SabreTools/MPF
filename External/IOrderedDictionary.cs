using System.Collections.Generic;
using System.Collections.Specialized;

namespace DICUI.External
{
    // Adapted from https://www.codeproject.com/Articles/18615/OrderedDictionary-T-A-generic-implementation-of-IO
    public interface IOrderedDictionary<TKey, TValue> : IOrderedDictionary, IDictionary<TKey, TValue>
    {
    }
}
