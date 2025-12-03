using System;
using System.Collections.Generic;

#if NET6_0_OR_GREATER
#else
using System.Linq;
#endif

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceEqualEqualityComparer<T> : IEqualityComparer<T[]>
    {
        /// <inheritdoc />
        public bool Equals(T[]? x, T[]? y)
        {
            return (x ?? Array.Empty<T>()).SequenceEqual(y ?? Array.Empty<T>());
        }

        /// <inheritdoc />
        public int GetHashCode(T[]? obj)
        {
            return 0;
        }
    }
}