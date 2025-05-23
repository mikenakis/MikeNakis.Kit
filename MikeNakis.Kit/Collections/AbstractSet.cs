namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit.Extensions;

/// Abstract base class for implementations of <see cref="ISet{T}"/>.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class AbstractSet<T> : AbstractReadOnlySet<T>, ISet<T>, IReadOnlySet<T> where T : notnull
{
	protected AbstractSet()
	{ }

	public abstract bool Add( T item );
	public abstract void Clear();
	public abstract bool Remove( T item );
	public abstract override IEnumerator<T> GetEnumerator();
	public abstract override bool Contains( T item );
	public abstract override int Count { get; }
	void ICollection<T>.Add( T item ) => Add( item );
	bool ICollection<T>.Contains( T item ) => Contains( item );
	int ICollection<T>.Count => Count;
	public bool IsReadOnly => false;
	public void CopyTo( T[] array, int arrayIndex ) => DotNetHelpers.CopyTo( this, array, arrayIndex );

	// From System.Collections.Generic.HashSet<T>
	public void UnionWith( IEnumerable<T> other )
	{
		foreach( T item in other )
			Add( item );
	}

	// From System.Collections.Generic.HashSet<T>
	public void IntersectWith( IEnumerable<T> other )
	{
		// Intersection of anything with empty set is empty set, so return if count is 0.
		// Same if the set intersecting with itself is the same set.
		if( Count == 0 || other.ReferenceEquals( this ) )
			return;

		// If other is known to be empty, intersection is empty set; remove all elements, and we're done.
		if( other is ICollection<T> otherAsCollection && otherAsCollection.Count == 0 )
		{
			Clear();
			return;
		}

		ISet<T> common = other.Where( Contains ).ToHashSet();
		foreach( T item in this.Collect() )
			if( !common.Contains( item ) )
				Remove( item );
	}

	// From System.Collections.Generic.HashSet<T>
	public void ExceptWith( IEnumerable<T> other )
	{
		// This is already the empty set; return.
		if( Count == 0 )
			return;

		// Special case if other is this; a set minus itself is the empty set.
		if( other.ReferenceEquals( this ) )
		{
			Clear();
			return;
		}

		// Remove every element in other from this.
		foreach( T element in other )
			Remove( element );
	}

	// From System.Collections.Generic.HashSet<T>
	public void SymmetricExceptWith( IEnumerable<T> other )
	{
		// If set is empty, then symmetric difference is other.
		if( Count == 0 )
		{
			UnionWith( other );
			return;
		}

		// Special-case this; the symmetric difference of a set with itself is the empty set.
		if( other.ReferenceEquals( this ) )
		{
			Clear();
			return;
		}

		HashSet<T> itemsToRemove = new();
		HashSet<T> itemsAddedFromOther = new();
		foreach( T item in other )
			if( Add( item ) )
				itemsAddedFromOther.Add( item );
			else if( !itemsAddedFromOther.Contains( item ) )
				itemsToRemove.Add( item );

		foreach( T item in itemsToRemove )
			Remove( item );
	}
}
