namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit;

public static class ListDictionary
{
	public static ListDictionary<V, K> InverseOf<K, V>( IReadOnlyDictionary<K, V> self ) where V : notnull
	{
		ListDictionary<V, K> inversed = new( capacity: self.Count );
		foreach( KeyValuePair<K, V> keyValuePair in self )
			inversed.Add( keyValuePair.Value, keyValuePair.Key );
		return inversed;
	}
}

/// An implementation of <see cref="IListDictionary{K,V}"/>.
/// Item deletion is O(N), other operations are as normal for a hash map.
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class ListDictionary<K, V> : AbstractDictionary<K, V>, IListDictionary<K, V> where K : notnull
{
	readonly MutableList<K> keys;
	readonly Dictionary<K, V> map;

	public ListDictionary()
	{
		keys = new MutableList<K>();
		map = new Dictionary<K, V>();
	}

	public ListDictionary( int capacity )
	{
		keys = new MutableList<K>( capacity );
		map = new Dictionary<K, V>( capacity );
	}

	public ListDictionary( IReadOnlyCollection<KeyValuePair<K, V>> pairs )
		: this( pairs.Count )
	{
		foreach( KeyValuePair<K, V> pair in pairs )
			Insert( -1, pair.Key, pair.Value );
	}

	public ListDictionary( IEnumerable<KeyValuePair<K, V>> pairs )
		: this()
	{
		foreach( KeyValuePair<K, V> pair in pairs )
			Insert( -1, pair.Key, pair.Value );
	}

	public bool IsEmpty() => Count == 0;
	public sealed override int Count => keys.Count;
	public override void Add( K key, V value ) => Insert( -1, key, value ); //TODO: make sealed
	public sealed override bool ContainsKey( K key ) => map.ContainsKey( key );

	public override bool Remove( K key ) //TODO: make sealed
	{
		if( !map.Remove( key ) )
			return false;
		keys.DoRemove( key );
		return true;
	}

	public sealed override bool TryGetValue( K key, out V value ) => map.TryGetValue( key, out value! );
	public sealed override IReadOnlyList<K> Keys => keys.AsReadOnlyList;
	public sealed override IReadOnlyList<V> Values => Keys.Select( key => map[key] );

	protected override bool Replace( K key, V value )
	{
		if( !map.ContainsKey( key ) )
			return false;
		map[key] = value;
		return true;
	}

	public void ReplaceKey( K oldKey, K newKey )
	{
		V value = map[oldKey];
		map.DoRemove( oldKey );
		map.Add( newKey, value );
		int index = keys.IndexOf( oldKey );
		keys[index] = newKey;
	}

	public void Insert( int index, K key, V value )
	{
		map.Add( key, value );
		keys.Insert( index == -1 ? Count : index, key );
	}

	public int IndexOf( KeyValuePair<K, V> item )
	{
		if( map.GetValueOrDefault( item.Key ) is null )
			return -1;
		return keys.IndexOf( item.Key );
	}

	public void Insert( int index, KeyValuePair<K, V> item ) => Insert( index, item.Key, item.Value );

	public void RemoveAt( int index )
	{
		K key = keys[index];
		keys.RemoveAt( index );
		map.DoRemove( key );
	}

	public KeyValuePair<K, V> this[int index]
	{
		get => keyValuePairOf( keys[index] );
		set
		{
			K oldKey = keys[index];
			ReplaceKey( oldKey, value.Key );
			this[value.Key] = value.Value;
		}
	}

	KeyValuePair<K, V> keyValuePairOf( K key ) => new( key, map[key] );
}
