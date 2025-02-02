namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using System.Linq;
using Sys = System;

/// Abstract base class for implementations of <see cref="IDictionary{K,V}"/>.
public abstract class AbstractDictionary<K, V> : AbstractReadOnlyDictionary<K, V>, IDictionary<K, V>, IReadOnlyDictionary<K, V> where K : notnull
{
	protected AbstractDictionary()
	{ }

	public virtual void Add( KeyValuePair<K, V> item )
	{
		Add( item.Key, item.Value );
	}

	public virtual void Clear()
	{
		foreach( K k in Keys.Collect() )
			Remove( k );
	}

	public virtual bool Remove( KeyValuePair<K, V> item )
	{
		if( !TryGetValue( item.Key, out V value ) )
			return false;
		if( !DotNetHelpers.Equals( item.Value, value ) )
			return false;
		Remove( item.Key );
		return true;
	}

	public virtual bool Contains( KeyValuePair<K, V> item )
	{
		if( !TryGetValue( item.Key, out V value ) )
			return false;
		return DotNetHelpers.Equals( item.Value, value );
	}

	public void CopyTo( KeyValuePair<K, V>[] array, int arrayIndex ) => DotNetHelpers.CopyTo( this, array, arrayIndex );
	public abstract override int Count { get; }
	public bool IsReadOnly => false;
	public abstract void Add( K key, V value );
	public abstract override bool ContainsKey( K key );
	public abstract bool Remove( K key );
	public abstract override bool TryGetValue( K key, out V value );
	protected abstract bool Replace( K key, V value );
	public new V this[K key] { get => GetValue( key ); set => AddOrReplace( key, value ); }
	public abstract override IEnumerable<K> Keys { get; } //TODO: this should be an ICollection
	public abstract override IEnumerable<V> Values { get; } //TODO: this should be an IReadOnlyCollection
	public override string ToString() => $"{Count} entries";
	ICollection<K> IDictionary<K, V>.Keys => new MakeshiftCollection<K>( Keys, () => Count, ContainsKey, _ => throw notImplemented(), _ => throw notImplemented(), () => throw notImplemented() );
	ICollection<V> IDictionary<K, V>.Values => new MakeshiftCollection<V>( Values, () => Count, v => Values.Contains( v ), _ => throw notImplemented(), _ => throw notImplemented(), () => throw notImplemented() );
	static Sys.Exception notImplemented() => new Sys.NotImplementedException();

	public void AddOrReplace( K key, V value )
	{
		if( ContainsKey( key ) )
		{
			bool ok = Replace( key, value );
			Assert( ok );
		}
		else
			Add( key, value );
	}
}
