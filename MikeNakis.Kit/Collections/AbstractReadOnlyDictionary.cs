namespace MikeNakis.Kit.Collections;

/// Abstract base class for implementations of <see cref="IReadOnlyDictionary{K,V}"/>.
public abstract class AbstractReadOnlyDictionary<K, V> : AbstractReadOnlyCollection<KeyValuePair<K, V>>, IReadOnlyDictionary<K, V> where K : notnull
{
	protected AbstractReadOnlyDictionary()
	{ }

	public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		foreach( K key in Keys )
			yield return new KeyValuePair<K, V>( key, this[key] );
	}

	public abstract bool ContainsKey( K key );
	public abstract bool TryGetValue( K key, out V value );
	public V this[K key] => GetValue( key );
	public abstract IEnumerable<K> Keys { get; }
	public abstract IEnumerable<V> Values { get; }

	public V GetValue( K key )
	{
		if( !TryGetValue( key, out V value ) )
			throw new KeyNotFoundException();
		return value;
	}
}
