namespace MikeNakis.Kit.Collections;

/// A mutable <see cref="IReadOnlyOrderedDictionary{K,V}"/>. Allows manipulating the order of keys.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IOrderedDictionary<K, V> : IDictionary<K, V>, IReadOnlyOrderedDictionary<K, V> where K : notnull
{
	void MoveFirst( K key );
	void MoveLast( K key );
}
