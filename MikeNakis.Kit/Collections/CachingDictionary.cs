namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using static MikeNakis.Kit.GlobalStatics;

/// An <see cref="IDictionary{K,V}" /> which evicts least-frequently-accessed items so as to never exceed a certain capacity.
// Based on https://stackoverflow.com/a/3719378/773113
public class CachingDictionary<K, V> : OrderedDictionary<K, V> where K : notnull
{
	readonly int capacity;

	public CachingDictionary( int capacity )
	{
		this.capacity = capacity;
	}

	public override bool TryGetValue( K key, out V value )
	{
		if( !base.TryGetValue( key, out value ) )
			return false;
		MoveLast( key );
		return true;
	}

	public override void Add( K key, V val )
	{
		base.Add( key, val );
		evictExcessKeys();
	}

	public new void AddBefore( K referenceKey, K key, V value )
	{
		base.AddBefore( referenceKey, key, value );
		evictExcessKeys();
	}

	public new void AddAfter( K referenceKey, K key, V value )
	{
		base.AddAfter( referenceKey, key, value );
		evictExcessKeys();
	}

	public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		//We have to override `GetEnumerator()` to avoid shuffling our own keys while trying
		//to enumerate, otherwise we get concurrent modification exception.
		foreach( K key in Keys )
		{
			bool ok = base.TryGetValue( key, out V value );
			Assert( ok );
			yield return new KeyValuePair<K, V>( key, value );
		}
	}

	void evictExcessKeys()
	{
		while( Count > capacity )
		{
			K? key = FirstKey;
			Assert( key is not null );
			this.DoRemove( key );
		}
	}
}
