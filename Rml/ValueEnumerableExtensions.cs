using ZLinq;
using ZLinq.Linq;

namespace Rml;

public static class ValueEnumerableExtensions
{
    public static ValueEnumerable<Zip<TEnumerator, Skip<TEnumerator, TSource>, TSource, TSource, (TSource previous, TSource current)>, (TSource previous, TSource current)> Pairwise<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source)
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif 
    {
        return source
            .Zip(source.Skip(1), (o, i) => (o, i));
    }
}