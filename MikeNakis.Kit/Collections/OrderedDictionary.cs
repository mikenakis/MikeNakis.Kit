namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using SysDiag = System.Diagnostics;

public static class OrderedDictionary
{
	public static OrderedDictionary<V, K> InverseOf<K, V>( IReadOnlyDictionary<K, V> self ) where V : notnull
	{
		OrderedDictionary<V, K> inversed = new( capacity: self.Count );
		foreach( KeyValuePair<K, V> keyValuePair in self )
			inversed.Add( keyValuePair.Value, keyValuePair.Key );
		return inversed;
	}
}

/// An implementation of <see cref="IOrderedDictionary{K,V}"/>.
/// Based on https://stackoverflow.com/a/3719378/773113
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class OrderedDictionary<K, V> : AbstractDictionary<K, V>, IOrderedDictionary<K, V> where K : notnull
{
	readonly Dictionary<K, LinkedListNode<(K, V)>> map;
	readonly LinkedList<(K key, V value)> list = new();

	public OrderedDictionary()
			: this( 0, Enumerable.Empty<KeyValuePair<K, V>>() )
	{ }

	public OrderedDictionary( int capacity )
			: this( capacity, Enumerable.Empty<KeyValuePair<K, V>>() )
	{ }

	public OrderedDictionary( IEnumerable<KeyValuePair<K, V>> keyValuePairs )
			: this( 0, keyValuePairs )
	{ }

	public OrderedDictionary( int capacity, IEnumerable<KeyValuePair<K, V>> keyValuePairs )
	{
		map = new( capacity );
		foreach( KeyValuePair<K, V> keyValuePair in keyValuePairs )
			add( keyValuePair.Key, keyValuePair.Value );
	}

	static bool validate => False;
	public override int Count => list.Count;
	public override bool ContainsKey( K key ) => map.ContainsKey( key );
	public override void Add( K key, V value ) => add( key, value );
	public override IReadOnlyOrderedSet<K> Keys => new KeyCollection( this );
	public override IReadOnlyCollection<V> Values => new MakeshiftReadOnlyCollection<V>( list.Select( node => node.value ), () => list.Count );
	public K? FirstKey => keyOrDefault( list.First );
	public K? NextKey( K key ) => keyOrDefault( map[key].Next );
	public K? LastKey => keyOrDefault( list.Last );
	public K? PreviousKey( K key ) => keyOrDefault( map[key].Previous );
	static K? keyOrDefault( LinkedListNode<(K key, V value)>? node ) => node == null ? default : node.Value.key;

	public override bool Remove( K key )
	{
		if( !map.TryGetValue( key, out LinkedListNode<(K key, V value)>? node ) )
			return false;
		Assert( Equals( node.Value.key, key ) );
		map.DoRemove( key );
		list.Remove( node );
		Assert( !validate || isValidAssertion() );
		return true;
	}

	public override bool TryGetValue( K key, out V value )
	{
		if( !map.TryGetValue( key, out LinkedListNode<(K key, V value)>? node ) )
		{
			value = default!;
			return false;
		}
		Assert( Equals( node.Value.key, key ) );
		value = node.Value.value;
		return true;
	}

	protected override void Replace( K key, V value )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		node.Value = (key, value);
		Assert( !validate || isValidAssertion() );
	}

	public void MoveFirst( K key )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		list.Remove( node );
		list.AddFirst( node );
		Assert( !validate || isValidAssertion() );
	}

	public void MoveLast( K key )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		list.Remove( node );
		list.AddLast( node );
		Assert( !validate || isValidAssertion() );
	}

	public void ReplaceKey( K key, K newKey )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		node.Value = (newKey, node.Value.value);
		map.DoRemove( key );
		map.Add( newKey, node );
	}

	void add( K key, V value )
	{
		LinkedListNode<(K, V)> node = new( (key, value) );
		map.Add( key, node );
		list.AddLast( node );
		Assert( !validate || isValidAssertion() );
	}

	bool isValidAssertion()
	{
		Assert( validate );
		foreach( (K key, LinkedListNode<(K key, V value)> node) in map )
		{
			Assert( Equals( node.Value.key, key ) );
			LinkedListNode<(K, V)>? foundNode = list.Find( node.Value );
			Assert( Equals( node, foundNode ) );
		}
		return true;
	}

	sealed class KeyCollection : AbstractReadOnlySet<K>, IReadOnlyOrderedSet<K>
	{
		readonly OrderedDictionary<K, V> orderedDictionary;

		internal KeyCollection( OrderedDictionary<K, V> orderedDictionary )
		{
			this.orderedDictionary = orderedDictionary;
		}

		public override IEnumerator<K> GetEnumerator() => orderedDictionary.list.Select( node => node.key ).GetEnumerator();
		public override bool Contains( K item ) => orderedDictionary.ContainsKey( item );
		public override int Count => orderedDictionary.list.Count;
		public K? First => keyOrDefault( orderedDictionary.list.First );
		public K? Next( K key ) => keyOrDefault( orderedDictionary.map[key].Next );
		public K? Last => keyOrDefault( orderedDictionary.list.Last );
		public K? Previous( K key ) => keyOrDefault( orderedDictionary.map[key].Previous );
	}
}
