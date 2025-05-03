namespace MikeNakis.Kit.Collections;

/// Abstract base class for implementations of <see cref="IList{T}"/>.
public abstract class AbstractList<T> : AbstractCollection<T>, IList<T>
{
	protected AbstractList()
	{ }

	public abstract T this[int index] { get; set; }

	public int IndexOf( T item )
	{
		int n = Count;
		for( int i = 0; i < n; i++ )
			if( Equals( item, this[i] ) )
				return i;
		return -1;
	}

	public override bool Contains( T item ) => IndexOf( item ) >= 0;
	public abstract void Insert( int index, T item );
	public abstract void RemoveAt( int index );
}
