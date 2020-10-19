using System;
using System.Collections.Generic;
using System.Linq;

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
            return obj?.GetHashCode() ?? 0;
        }
    }
}