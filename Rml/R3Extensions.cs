using R3;

namespace Rml;

public static class R3Extensions
{
    public static BindableReactiveProperty<T?> ToBindableReactivePropertyAsSynchronized<T>(
        this ReactiveProperty<T?> source, ref DisposableBuilder db)
    {
        var result = source.ToBindableReactiveProperty();
        result.Subscribe(source, (o, state) => state.Value = o).AddTo(ref db);
        return result;
    }
}