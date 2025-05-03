namespace MikeNakis.Kit.Collections;

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
/// Original inspiration from https://stackoverflow.com/a/3719378/773113
#if DEBUG
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( OrderedDictionaryDebugView<,> ) )]
#endif
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
			addLast( keyValuePair.Key, keyValuePair.Value );
	}

	const bool validate = false;
	public override int Count => list.Count;
	public override bool ContainsKey( K key ) => map.ContainsKey( key );
	public override void Add( K key, V value ) => addLast( key, value );
	public override IReadOnlyOrderedSet<K> Keys => new KeyCollection( this );
	public override IReadOnlyCollection<V> Values => new MakeshiftReadOnlyCollection<V>( list.Select( node => node.value ), () => list.Count );
	public K? FirstKey => keyOrDefault( list.First );
	public K? NextKey( K key ) => keyOrDefault( map[key].Next );
	public K? LastKey => keyOrDefault( list.Last );
	public K? PreviousKey( K key ) => keyOrDefault( map[key].Previous );
	static K? keyOrDefault( LinkedListNode<(K key, V value)>? node ) => node == null ? default : node.Value.key;

	public void AddBefore( K referenceKey, K key, V value )
	{
		LinkedListNode<(K, V)> newNode = new( (key, value) );
		LinkedListNode<(K, V)> referenceNode = map[referenceKey];
		map.Add( key, newNode );
		referenceNode.List!.AddBefore( referenceNode, newNode );
		Assert( !validate || isValidAssertion() );
	}

	public void AddAfter( K referenceKey, K key, V value )
	{
		LinkedListNode<(K, V)> newNode = new( (key, value) );
		LinkedListNode<(K, V)> referenceNode = map[referenceKey];
		map.Add( key, newNode );
		referenceNode.List!.AddAfter( referenceNode, newNode );
		Assert( !validate || isValidAssertion() );
	}

	public void MoveBefore( K referenceKey, K key )
	{
		if( referenceKey.Equals( key ) )
			return;
		LinkedListNode<(K, V)> node = map[key];
		LinkedListNode<(K, V)> referenceNode = map[referenceKey];
		node.List!.Remove( node );
		referenceNode.List!.AddBefore( referenceNode, node );
		Assert( !validate || isValidAssertion() );
	}

	public void MoveAfter( K referenceKey, K key )
	{
		if( referenceKey.Equals( key ) )
			return;
		LinkedListNode<(K, V)> node = map[key];
		LinkedListNode<(K, V)> referenceNode = map[referenceKey];
		node.List!.Remove( node );
		referenceNode.List!.AddAfter( referenceNode, node );
		Assert( !validate || isValidAssertion() );
	}

	public override bool Remove( K key )
	{
		if( !map.TryGetValue( key, out LinkedListNode<(K key, V value)>? node ) )
			return false;
		Assert( Equals( node.Value.key, key ) );
		map.DoRemove( key );
		node.List!.Remove( node );
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

	protected override bool Replace( K key, V value )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		if( !Equals( node.Value.key, key ) )
			return false;
		node.Value = (key, value);
		Assert( !validate || isValidAssertion() );
		return true;
	}

	public void MoveFirst( K key )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		node.List!.Remove( node );
		list.AddFirst( node );
		Assert( !validate || isValidAssertion() );
	}

	public void MoveLast( K key )
	{
		LinkedListNode<(K key, V value)> node = map[key];
		Assert( Equals( node.Value.key, key ) );
		node.List!.Remove( node );
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

	void addLast( K key, V value )
	{
		LinkedListNode<(K, V)> newNode = new( (key, value) );
		map.Add( key, newNode );
		list.AddLast( newNode );
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

	public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		return list.Select( tuple => new KeyValuePair<K, V>( tuple.key, tuple.value ) ).GetEnumerator();
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
		public override int Count => orderedDictionary.Count;
		public K? First => orderedDictionary.FirstKey;
		public K? Next( K key ) => orderedDictionary.NextKey( key );
		public K? Last => orderedDictionary.LastKey;
		public K? Previous( K key ) => orderedDictionary.PreviousKey( key );
	}
}

#if DEBUG
sealed class OrderedDictionaryDebugView<K, V> where K : notnull
{
	[SysDiag.DebuggerBrowsable( SysDiag.DebuggerBrowsableState.RootHidden )]
#pragma warning disable CA1819 // Properties should not return arrays
	//This property will be invoked by the debugger.
	public object[] Items => getDetails();
#pragma warning restore CA1819 // Properties should not return arrays

	object[] getDetails()
	{
		int index = 0;
		return orderedDictionary.Select( pair => new OrderedDictionaryDebugViewDetail( index++, pair.Key, pair.Value ) ).ToArraySeriously();
	}

	readonly OrderedDictionary<K, V> orderedDictionary;

	public OrderedDictionaryDebugView( OrderedDictionary<K, V> orderedDictionary )
	{
		this.orderedDictionary = orderedDictionary;
	}
}

[SysDiag.DebuggerDisplay( value: "{ToString(),nq}", Name = "{GetKey(),nq}", Type = "{GetTypeString(),nq}" )]
sealed class OrderedDictionaryDebugViewDetail
{
	[SysDiag.DebuggerBrowsable( SysDiag.DebuggerBrowsableState.Never )] readonly int index;
	public object Key { get; }
	public object? Value { get; }

	public OrderedDictionaryDebugViewDetail( int index, object key, object? value )
	{
		this.index = index;
		Key = key;
		Value = value;
	}

	public string GetKey() => $"#{index} [{Key}]";

	public string GetTypeString() => Value?.GetType().FullName ?? "";

	public override string ToString() => Value == null ? "null" : Value.ToString() ?? "";
}
#endif
