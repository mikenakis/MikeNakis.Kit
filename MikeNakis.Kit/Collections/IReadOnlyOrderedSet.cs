namespace MikeNakis.Kit.Collections;

/// An ordered <see cref="IReadOnlySet{T}" />. Allows obtaining elements in order.
/// Note: the presence of the `Next` and `Previous` methods prevents T from being an `out` parameter.
public interface IReadOnlyOrderedSet<T> : IReadOnlySet<T> where T : notnull
{
	T? First { get; }
	T? Next( T item );
	T? Last { get; }
	T? Previous( T item );
}
