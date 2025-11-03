namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using SysDiag = System.Diagnostics;

/// Abstract base class for implementations of <see cref="IReadOnlySet{T}"/>.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class AbstractReadOnlySet<T> : AbstractReadOnlyCollection<T>, IReadOnlySet<T> where T : notnull
{
	// From System.Collections.Generic.HashSet<T>
	protected const int StackAllocationThreshold = 100;

	protected AbstractReadOnlySet()
	{ }

	public abstract override IEnumerator<T> GetEnumerator();
	public abstract bool Contains( T item );
	public abstract override int Count { get; }
	public override string ToString() => $"{Count} items";

	// From System.Collections.Generic.HashSet<T>
	public bool IsSubsetOf( IEnumerable<T> other )
	{
		// The empty set is a subset of any set, and a set is a subset of itself.
		// Set is always a subset of itself
		if( Count == 0 || other.ReferenceEquals( this ) )
			return true;

		(int uniqueCount, int unfoundCount) = checkUniqueAndUnfoundElements( other, returnIfUnfound: false );
		return uniqueCount == Count && unfoundCount >= 0;
	}

	// From System.Collections.Generic.HashSet<T>
	public bool IsProperSubsetOf( IEnumerable<T> other )
	{
		// No set is a proper subset of itself.
		if( other.ReferenceEquals( this ) )
			return false;

		if( other is ICollection<T> otherAsCollection )
		{
			// No set is a proper subset of an empty set.
			if( otherAsCollection.Count == 0 )
				return false;

			// The empty set is a proper subset of anything but the empty set.
			if( Count == 0 )
				return otherAsCollection.Count > 0;
		}

		(int uniqueCount, int unfoundCount) = checkUniqueAndUnfoundElements( other, returnIfUnfound: false );
		return uniqueCount == Count && unfoundCount > 0;
	}

	// From System.Collections.Generic.HashSet<T>
	public bool IsSupersetOf( IEnumerable<T> other )
	{
		// A set is always a superset of itself.
		if( other.ReferenceEquals( this ) )
			return true;

		// Try to fall out early based on counts.
		if( other is ICollection<T> otherAsCollection )
		{
			// If other is the empty set then this is a superset.
			if( otherAsCollection.Count == 0 )
				return true;
		}

		foreach( T element in other )
			if( !Contains( element ) )
				return false;

		return true;
	}

	// From System.Collections.Generic.HashSet<T>
	public bool IsProperSupersetOf( IEnumerable<T> other )
	{
		// The empty set isn't a proper superset of any set, and a set is never a strict superset of itself.
		if( Count == 0 || other.ReferenceEquals( this ) )
			return false;

		if( other is ICollection<T> otherAsCollection )
		{
			// If other is the empty set then this is a superset.
			if( otherAsCollection.Count == 0 )
				return true; // Note that this has at least one element, based on above check.
		}

		// Couldn't fall out in the above cases; do it the long way
		(int uniqueCount, int unfoundCount) = checkUniqueAndUnfoundElements( other, returnIfUnfound: true );
		return uniqueCount < Count && unfoundCount == 0;
	}

	// From System.Collections.Generic.HashSet<T>
	public bool Overlaps( IEnumerable<T> other )
	{
		if( Count == 0 )
			return false;

		// Set overlaps itself
		if( other.ReferenceEquals( this ) )
			return true;

		foreach( T element in other )
			if( Contains( element ) )
				return true;

		return false;
	}

	// From System.Collections.Generic.HashSet<T>
	public bool SetEquals( IEnumerable<T> other )
	{
		// A set is equal to itself.
		if( other.ReferenceEquals( this ) )
			return true;

		// If this count is 0 but other contains at least one element, they can't be equal.
		if( Count == 0 && other is ICollection<T> otherAsCollection && otherAsCollection.Count > 0 )
			return false;

		(int uniqueCount, int unfoundCount) = checkUniqueAndUnfoundElements( other, returnIfUnfound: true );
		return uniqueCount == Count && unfoundCount == 0;
	}

	(int UniqueCount, int UnfoundCount) checkUniqueAndUnfoundElements( IEnumerable<T> other, bool returnIfUnfound )
	{
		// Need special case in case this has no elements.
		if( Count == 0 )
		{
			//TODO: replace this with other.Any();
			int numElementsInOther = 0;
			foreach( T _ in other )
			{
				numElementsInOther++;
				break; // break right away, all we want to know is whether other has 0 or 1 elements
			}

			return (UniqueCount: 0, UnfoundCount: numElementsInOther);
		}

		HashSet<T> found = new();

		int unfoundCount = 0; // count of items in other not found in this
		int uniqueFoundCount = 0; // count of unique items in other found in this

		foreach( T item in other )
		{
			if( Contains( item ) )
			{
				if( found.Add( item ) )
					uniqueFoundCount++;
			}
			else
			{
				unfoundCount++;
				if( returnIfUnfound )
					break;
			}
		}

		return (uniqueFoundCount, unfoundCount);
	}

	protected int FindItemIndex( T itemToFind )
	{
		int index = 0;
		foreach( T item in this )
		{
			if( Equals( item, itemToFind ) )
				return index;
			index++;
		}
		return -1;
	}
}
