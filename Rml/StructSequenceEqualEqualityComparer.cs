using System;
using System.Collections.Generic;

namespace Rml
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StructSequenceEqualEqualityComparer<T> : IEqualityComparer<T[]>
        where T : struct, IEquatable<T>
    {
        /// <inheritdoc />
        public bool Equals(T[]? x, T[]? y)
        {
            ReadOnlySpan<T> xSpan = x ?? Array.Empty<T>();
            ReadOnlySpan<T> ySpan = y ?? Array.Empty<T>();
            return xSpan.SequenceEqual(ySpan);
        }

        /// <inheritdoc />
        public int GetHashCode(T[]? obj)
        {
            return 0;
        }
    }
}