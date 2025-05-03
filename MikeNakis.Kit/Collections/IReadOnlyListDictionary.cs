namespace MikeNakis.Kit.Collections;

/// A list <see cref="IReadOnlyDictionary{K,V}"/>.  Allows indexed access to keys and key-value-pairs.
public interface IReadOnlyListDictionary<K, V> : IReadOnlyDictionary<K, V>, IReadOnlyList<KeyValuePair<K, V>> where K : notnull
{
	new IReadOnlyList<K> Keys { get; }
	new IReadOnlyList<V> Values { get; }
}
