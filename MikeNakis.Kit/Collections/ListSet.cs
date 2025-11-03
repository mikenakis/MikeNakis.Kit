namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using SysDiag = System.Diagnostics;

/// An implementation of <see cref="IListSet{T}"/>.
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public class ListSet<T> : AbstractSet<T>, IReadOnlyList<T> where T : notnull
{
	readonly MutableList<T> list = new();
	readonly HashSet<T> set = new();

	public ListSet()
	{ }

	public ListSet( IEnumerable<T> items )
	{
		foreach( T item in items )
		{
			bool ok = set.Add( item );
			Assert( ok );
			list.Add( item );
		}
	}

	public override int Count => list.Count;

	public override bool Add( T item )
	{
		if( !set.Add( item ) )
			return false;
		list.Add( item );
		return true;
	}

	public override void Clear()
	{
		set.Clear();
		list.Clear();
	}

	public override IEnumerator<T> GetEnumerator() => list.GetEnumerator();

	public override bool Contains( T item )
	{
		bool result = set.Contains( item );
		Assert( result == (list.IndexOf( item ) != -1) );
		return result;
	}

	public override bool Remove( T item )
	{
		if( !set.Remove( item ) )
			return false;
		bool ok = list.Remove( item );
		Assert( ok );
		return true;
	}

	public T this[int index] => list[index];

	public void Replace( T oldItem, T newItem )
	{
		Assert( Contains( oldItem ) );
		Assert( !Contains( newItem ) );
		int index = list.IndexOf( oldItem );
		Assert( index != -1 );
		bool ok = set.Remove( oldItem );
		Assert( ok );
		ok = set.Add( newItem );
		Assert( ok );
		list[index] = newItem;
		Assert( !Contains( oldItem ) );
		Assert( Contains( newItem ) );
	}
}
