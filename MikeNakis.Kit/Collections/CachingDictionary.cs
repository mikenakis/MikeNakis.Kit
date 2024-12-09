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
		while( Count >= capacity )
			removeOldest();
	}

	void removeOldest()
	{
		Assert( Count > 0 );
		K? key = FirstKey;
		if( key is null )
			return;
		bool ok = Remove( key );
		Assert( ok );
	}
}
