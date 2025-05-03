namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit;

/// An add-only list implemented as an immutable linked list, using structural sharing.
/// An instance of this class is also a node of the linked list.
/// Since each node contains a reference to the previous node, each node is also a tail.
[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public abstract class Sequence<T> : IEnumerable<T>
{
	public static Sequence<T> Of() => new HeadSequence<T>();
	public static Sequence<T> Of( T item ) => Of().Add( item );
	public static Sequence<T> Of( T item1, T item2 ) => Of().Add( item1 ).Add( item2 );

	private protected Sequence() { }

	[SysContracts.Pure] public Sequence<T> Add( T item ) => new TailSequence<T>( this, item );

	public IReadOnlyList<T> Collect()
	{
		MutableList<T> mutableList = new();
		//NOTE: we cannot use recursion here because a long list will cause stack overflow.
		Sequence<T> sequence = this;
		for( ; true; )
		{
			(T item, Sequence<T> previous)? tuple = sequence.ItemAndPrevious;
			if( !tuple.HasValue )
				break;
			mutableList.Add( tuple.Value.item );
			sequence = tuple.Value.previous;
		}
		return mutableList.AsReadOnlyList;
	}

	public int Count
	{
		get
		{
			int count = 0;
			//NOTE: we cannot use recursion here because a long list will cause stack overflow.
			Sequence<T> sequence = this;
			for( ; true; )
			{
				(T item, Sequence<T> previous)? tuple = sequence.ItemAndPrevious;
				if( !tuple.HasValue )
					break;
				count++;
				sequence = tuple.Value.previous;
			}
			return count;
		}
	}

	public abstract bool IsEmpty();
	internal abstract (T item, Sequence<T> previous)? ItemAndPrevious { get; }
	public IEnumerator<T> GetEnumerator() => Collect().GetEnumerator();
	LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class HeadSequence<T> : Sequence<T>
{
	internal HeadSequence() { }

	public override bool IsEmpty() => true;
	internal override (T item, Sequence<T> previous)? ItemAndPrevious => null;
}

public sealed class TailSequence<T> : Sequence<T>
{
	readonly Sequence<T> previous;
	readonly T item;

	internal TailSequence( Sequence<T> previous, T item )
	{
		this.previous = previous;
		this.item = item;
	}

	public override bool IsEmpty() => false;
	internal override (T item, Sequence<T> previous)? ItemAndPrevious => (item, previous);
}
