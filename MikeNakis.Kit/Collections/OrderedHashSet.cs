namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using SysDiag = System.Diagnostics;

/// An implementation of <see cref="IOrderedSet{T}"/>.
/// Based on https://stackoverflow.com/a/3719378/773113
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class OrderedHashSet<T> : AbstractSet<T>, IOrderedSet<T> where T : notnull
{
	readonly Dictionary<T, LinkedListNode<T>> map;
	readonly LinkedList<T> list = new();

	public OrderedHashSet()
			: this( EqualityComparer<T>.Default )
	{ }

	public OrderedHashSet( IEqualityComparer<T>? comparer )
	{
		map = new Dictionary<T, LinkedListNode<T>>( 0, comparer );
	}

	public OrderedHashSet( IEqualityComparer<T>? comparer, IEnumerable<T> items )
			: this( comparer )
	{
		foreach( T item in items )
			add( item );
	}

	public OrderedHashSet( IEnumerable<T> items )
			: this()
	{
		foreach( T item in items )
			add( item );
	}

	bool add( T item )
	{
		if( map.ContainsKey( item ) )
			return false;
		LinkedListNode<T> node = new( item );
		map.Add( item, node );
		list.AddLast( node );
		return true;
	}

	public override bool Add( T item ) => add( item );
	public override int Count => list.Count;
	public override bool Contains( T item ) => map.ContainsKey( item );
	public override IEnumerator<T> GetEnumerator() => list.GetEnumerator();
	public T? First => valueOrDefault( list.First );
	public T? Next( T item ) => valueOrDefault( map[item].Next );
	public T? Last => valueOrDefault( list.Last );
	public T? Previous( T item ) => valueOrDefault( map[item].Previous );

	public override void Clear()
	{
		map.Clear();
		list.Clear();
	}

	public override bool Remove( T item )
	{
		if( !map.TryGetValue( item, out LinkedListNode<T>? node ) )
			return false;
		map.DoRemove( item );
		list.Remove( node );
		return true;
	}

	public void MoveFirst( T item )
	{
		LinkedListNode<T> node = map[item];
		list.Remove( node );
		list.AddFirst( node );
	}

	public void MoveLast( T item )
	{
		LinkedListNode<T> node = map[item];
		list.Remove( node );
		list.AddLast( node );
	}

	static T? valueOrDefault( LinkedListNode<T>? node ) => node == null ? default : node.Value;
}
