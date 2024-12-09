namespace MikeNakis.Kit.Collections;

/// A list <see cref="IDictionary{K,V}"/>.  Offers indexed access to keys and key-value-pairs.
// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IListDictionary<K, V> : IList<KeyValuePair<K, V>>, IDictionary<K, V>, IReadOnlyListDictionary<K, V> where K : notnull
{
	void Insert( int index, K key, V value );
	new KeyValuePair<K, V> this[int index] { get; set; }
}
