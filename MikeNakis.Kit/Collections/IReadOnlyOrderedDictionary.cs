namespace MikeNakis.Kit.Collections;

/// An ordered <see cref="IReadOnlyDictionary{K,V}"/>. Allows obtaining keys in order.
public interface IReadOnlyOrderedDictionary<K, V> : IReadOnlyDictionary<K, V> where K : notnull
{
	new IReadOnlyOrderedSet<K> Keys { get; }
	K? FirstKey { get; }
	K? NextKey( K key );
	K? LastKey { get; }
	K? PreviousKey( K key );
}
