namespace MikeNakis.Kit.Collections;

using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public struct MutableArray<T>
{
	const int DefaultCapacity = 4;

	T[] array;
	int size;

	public MutableArray()
	{
		array = Sys.Array.Empty<T>();
	}

	public readonly int Count => size;

	// [Sys.Obsolete]
	// public readonly override bool Equals( object? other )
	// {
	// 	return other switch
	// 	{
	// 		MutableSeries<T> mutableSeries => Equals( mutableSeries ),
	// 		Series<T> series => Equals( series ),
	// 		IReadOnlyList<T> enumerable => Equals( enumerable ),
	// 		IEnumerable<T> enumerable => Equals( enumerable ),
	// 		_ => false
	// 	};
	// }
	//
	// public readonly bool Equals( MutableSeries<T> other )
	// {
	// 	int count = Count;
	// 	if( count != other.Count )
	// 		return false;
	// 	EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
	// 	for( int i = 0; i < count; i++ )
	// 		if( !equalityComparer.Equals( array[i], other[i] ) )
	// 			return false;
	// 	return true;
	// }
	//
	// public readonly bool Equals( Series<T> other )
	// {
	// 	int count = Count;
	// 	if( count != other.Length )
	// 		return false;
	// 	EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
	// 	for( int i = 0; i < count; i++ )
	// 		if( !equalityComparer.Equals( array[i], other[i] ) )
	// 			return false;
	// 	return true;
	// }
	//
	// public readonly bool Equals( IReadOnlyList<T> other )
	// {
	// 	int count = Count;
	// 	if( count != other.Count )
	// 		return false;
	// 	EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
	// 	for( int i = 0; i < count; i++ )
	// 	{
	// 		T rightItem = other[i];
	// 		if( !equalityComparer.Equals( array[i], rightItem ) )
	// 			return false;
	// 	}
	// 	return true;
	// }
	//
	// public readonly bool Equals( IEnumerable<T> other )
	// {
	// 	EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
	// 	if( False )
	// 	{
	// 		Enumerator selfEnumerator = GetEnumerator();
	// 		IEnumerator<T> otherEnumerator = other.GetEnumerator();
	// 		try
	// 		{
	// 			while( true )
	// 			{
	// 				bool selfHasNext = selfEnumerator.MoveNext();
	// 				bool otherHasNext = otherEnumerator.MoveNext();
	// 				if( !selfHasNext && !otherHasNext )
	// 					break;
	// 				if( selfHasNext || otherHasNext )
	// 					return false;
	// 				T selfItem = selfEnumerator.Current;
	// 				T otherItem = otherEnumerator.Current;
	// 				if( !equalityComparer.Equals( selfItem, otherItem ) )
	// 					return false;
	// 			}
	// 		}
	// 		finally
	// 		{
	// 			otherEnumerator.Dispose();
	// 			selfEnumerator.Dispose();
	// 		}
	// 	}
	// 	else if( False )
	// 	{
	// 		IEnumerator<T> otherEnumerator = other.GetEnumerator();
	// 		try
	// 		{
	// 			foreach( T selfItem in this )
	// 			{
	// 				if( !otherEnumerator.MoveNext() )
	// 					return false;
	// 				T otherItem = otherEnumerator.Current;
	// 				if( !equalityComparer.Equals( selfItem, otherItem ) )
	// 					return false;
	// 			}
	// 			if( otherEnumerator.MoveNext() )
	// 				return false;
	// 		}
	// 		finally
	// 		{
	// 			otherEnumerator.Dispose();
	// 		}
	// 	}
	// 	else
	// 	{
	// 		IEnumerator<T> otherEnumerator = other.GetEnumerator();
	// 		try
	// 		{
	// 			for( int i = 0; i < Count; i++ )
	// 			{
	// 				if( !otherEnumerator.MoveNext() )
	// 					return false;
	// 				T otherItem = otherEnumerator.Current;
	// 				if( !equalityComparer.Equals( array[i], otherItem ) )
	// 					return false;
	// 			}
	// 			if( otherEnumerator.MoveNext() )
	// 				return false;
	// 		}
	// 		finally
	// 		{
	// 			otherEnumerator.Dispose();
	// 		}
	// 	}
	// 	return true;
	// }
	//
	// public readonly override int GetHashCode()
	// {
	// 	Sys.HashCode hashCodeBuilder = new();
	// 	// ReSharper disable once NonReadonlyMemberInGetHashCode
	// 	foreach( T item in array )
	// 		hashCodeBuilder.Add( item );
	// 	return hashCodeBuilder.ToHashCode();
	// }

	public int Capacity
	{
		readonly get => array.Length;
		set
		{
			Assert( value >= Count );
			if( value == array.Length )
				return;
			if( value <= 0 )
			{
				Assert( false ); //I do not think this can ever happen.
				array = Sys.Array.Empty<T>();
			}
			else
			{
				T[] newItems = new T[value];
				if( Count > 0 )
					Sys.Array.Copy( array, newItems, Count );
				array = newItems;
			}
		}
	}

	// readonly bool ICollection<T>.IsReadOnly => false;

	public T this[int index]
	{
		readonly get
		{
			Assert( (uint)index < (uint)Count );
			return array[index];
		}
		set
		{
			Assert( (uint)index < (uint)Count );
			array[index] = value;
		}
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public void Add( T item )
	{
		T[] array = this.array;
		int size = Count;
		if( (uint)size < (uint)array.Length )
		{
			this.size = size + 1;
			array[size] = item;
		}
		else
			addWithResize( item );
	}

	// Non-inline from List.Add to improve its code quality as uncommon path
	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.NoInlining )]
	void addWithResize( T item )
	{
		SysDiag.Debug.Assert( Count == array.Length );
		int size = Count;
		grow( size + 1 );
		this.size = size + 1;
		array[size] = item;
	}

	public void Clear()
	{
		if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
		{
			int size = Count;
			this.size = 0;
			if( size > 0 )
				Sys.Array.Clear( array, 0, size ); // Clear the elements so that the gc can reclaim the references.
		}
		else
			size = 0;
	}

	public readonly bool Contains( T item ) => Count != 0 && IndexOf( item ) >= 0;

	// public readonly void CopyTo( T[] array ) => CopyTo( array, 0 );
	//
	// public readonly void CopyTo( int index, T[] array, int arrayIndex, int count )
	// {
	// 	Assert( Count - index >= count );
	// 	Sys.Array.Copy( this.array, index, array, arrayIndex, count );
	// }
	//
	// public readonly void CopyTo( T[] array, int arrayIndex )
	// {
	// 	Sys.Array.Copy( this.array, 0, array, arrayIndex, Count );
	// }

	public int EnsureCapacity( int capacity )
	{
		Assert( capacity >= 0 );
		if( array.Length < capacity )
			grow( capacity );
		return array.Length;
	}

	void grow( int capacity )
	{
		Assert( array.Length < capacity );

		int newCapacity = array.Length == 0 ? DefaultCapacity : 2 * array.Length;

		// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
		// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
		if( (uint)newCapacity > Sys.Array.MaxLength )
			newCapacity = Sys.Array.MaxLength;

		// If the computed capacity is still less than specified, set to the original argument.
		// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
		if( newCapacity < capacity )
			newCapacity = capacity;

		Capacity = newCapacity;
	}

	// public readonly bool Exists( Sys.Predicate<T> match ) => FindIndex( match ) != -1;

	// public readonly T? Find( Sys.Predicate<T> match )
	// {
	// 	for( int i = 0; i < Count; i++ )
	// 		if( match( array[i] ) )
	// 			return array[i];
	// 	return default;
	// }

	// public readonly int FindIndex( Sys.Predicate<T> match ) => FindIndex( 0, Count, match );
	// public readonly int FindIndex( int startIndex, Sys.Predicate<T> match ) => FindIndex( startIndex, Count - startIndex, match );
	//
	// public readonly int FindIndex( int startIndex, int count, Sys.Predicate<T> match )
	// {
	// 	Assert( (uint)startIndex <= (uint)Count );
	// 	Assert( count >= 0 && startIndex <= Count - count );
	// 	int endIndex = startIndex + count;
	// 	for( int i = startIndex; i < endIndex; i++ )
	// 	{
	// 		if( match( array[i] ) )
	// 			return i;
	// 	}
	// 	return -1;
	// }
	//
	// public readonly T? FindLast( Sys.Predicate<T> match )
	// {
	// 	for( int i = Count - 1; i >= 0; i-- )
	// 		if( match( array[i] ) )
	// 			return array[i];
	// 	return default;
	// }
	//
	// public readonly int FindLastIndex( Sys.Predicate<T> match ) => FindLastIndex( Count - 1, Count, match );
	//
	// public readonly int FindLastIndex( int startIndex, Sys.Predicate<T> match ) => FindLastIndex( startIndex, startIndex + 1, match );
	//
	// public readonly int FindLastIndex( int startIndex, int count, Sys.Predicate<T> match )
	// {
	// 	Assert( Count == 0 ? startIndex == -1 : (uint)startIndex < (uint)Count );
	// 	Assert( count >= 0 && startIndex - count + 1 >= 0 );
	//
	// 	int endIndex = startIndex - count;
	// 	for( int i = startIndex; i > endIndex; i-- )
	// 		if( match( array[i] ) )
	// 			return i;
	// 	return -1;
	// }
	//
	// public readonly void ForEach( Sys.Action<T> action )
	// {
	// 	int version = this.version;
	// 	for( int i = 0; i < Count; i++ )
	// 	{
	// 		if( version != this.version )
	// 			break;
	// 		action( array[i] );
	// 	}
	// 	if( version != this.version )
	// 		throw new Sys.InvalidOperationException();
	// }

	//public readonly Enumerator GetEnumerator() => new( this );

	// readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	//
	// readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

	public readonly int IndexOf( T item ) => Sys.Array.IndexOf( array, item, 0, Count );

	public readonly int IndexOf( T item, int index )
	{
		Assert( index <= Count );
		return Sys.Array.IndexOf( array, item, index, Count - index );
	}

	public readonly int IndexOf( T item, int index, int count )
	{
		Assert( index <= Count );
		Assert( count >= 0 && index <= Count - count );
		return Sys.Array.IndexOf( array, item, index, count );
	}

	public void Insert( int index, T item )
	{
		Assert( (uint)index <= (uint)Count ); // Note that insertions at the end are legal.
		if( Count == array.Length )
			grow( Count + 1 );
		if( index < Count )
			Sys.Array.Copy( array, index, array, index + 1, Count - index );
		array[index] = item;
		size++;
	}

	public void InsertRange( int index, IEnumerable<T> enumerable )
	{
		Assert( (uint)index <= (uint)Count );
		if( enumerable is IReadOnlyCollection<T> readOnlyCollection )
		{
			int count = readOnlyCollection.Count;
			if( count > 0 )
			{
				if( array.Length - Count < count )
					grow( checked(Count + count) );
				if( index < Count )
					Sys.Array.Copy( array, index, array, index + count, Count - index );
				foreach( T item in readOnlyCollection )
					array[index++] = item;
				size += count;
			}
		}
		else if( enumerable is ICollection<T> collection )
		{
			int count = collection.Count;
			if( count > 0 )
			{
				if( array.Length - Count < count )
					grow( checked(Count + count) );
				if( index < Count )
					Sys.Array.Copy( array, index, array, index + count, Count - index );
				collection.CopyTo( array, index );
				size += count;
			}
		}
		else
			foreach( T item in enumerable )
				Insert( index++, item );
	}

	// public readonly int LastIndexOf( T item ) => Count == 0 ? -1 : LastIndexOf( item, Count - 1, Count ); //TODO: conditional operator is probably unnecessary here
	//
	// public readonly int LastIndexOf( T item, int index )
	// {
	// 	Assert( index < Count );
	// 	return LastIndexOf( item, index, index + 1 );
	// }
	//
	// public readonly int LastIndexOf( T item, int index, int count )
	// {
	// 	Assert( Count == 0 || index >= 0 );
	// 	Assert( Count == 0 || count >= 0 );
	// 	if( Count == 0 ) // Special case for empty list
	// 		return -1;
	// 	Assert( index < Count );
	// 	Assert( count <= index + 1 );
	// 	return Sys.Array.LastIndexOf( array, item, index, count );
	// }

	public bool Remove( T item )
	{
		int index = IndexOf( item );
		if( index >= 0 )
		{
			RemoveAt( index );
			return true;
		}
		return false;
	}

	public int RemoveAll( Sys.Predicate<T> match )
	{
		int freeIndex = 0; // the first free slot in items array
		while( freeIndex < Count && !match( array[freeIndex] ) ) // Find the first item which needs to be removed.
			freeIndex++;
		if( freeIndex >= Count )
			return 0;
		int current = freeIndex + 1;
		while( current < Count )
		{
			while( current < Count && match( array[current] ) ) // Find the first item which needs to be kept.
				current++;
			if( current < Count )
				array[freeIndex++] = array[current++]; // copy item to the free slot.
		}
		if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
			Sys.Array.Clear( array, freeIndex, Count - freeIndex ); // Clear the elements so that the gc can reclaim the references.
		int result = Count - freeIndex;
		size = freeIndex;
		return result;
	}

	public void RemoveAt( int index )
	{
		Assert( (uint)index < (uint)Count );
		size--;
		if( index < Count )
			Sys.Array.Copy( array, index + 1, array, index, Count - index );
		if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
			array[Count] = default!;
	}

	public void RemoveRange( int index, int count )
	{
		Assert( index >= 0 );
		Assert( count >= 0 );
		Assert( Count - index >= count );
		if( count > 0 )
		{
			size -= count;
			if( index < Count )
				Sys.Array.Copy( array, index + count, array, index, Count - index );
			if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
				Sys.Array.Clear( array, Count, count );
		}
	}

	// public void Reverse() => Reverse( 0, Count );
	//
	// public void Reverse( int index, int count )
	// {
	// 	Assert( index >= 0 );
	// 	Assert( count >= 0 );
	// 	Assert( Count - index >= count );
	// 	if( count > 1 )
	// 		Sys.Array.Reverse( array, index, count );
	// 	version++;
	// }

	public void Sort() => Sort( 0, Count, null );

	public void Sort( IComparer<T>? comparer ) => Sort( 0, Count, comparer );

	public void Sort( int index, int count, IComparer<T>? comparer )
	{
		Assert( index >= 0 );
		Assert( count >= 0 );
		Assert( Count - index >= count );
		if( count > 1 )
			Sys.Array.Sort( array, index, count, comparer );
	}

	public void Sort( Sys.Comparison<T> comparison )
	{
		if( Count > 1 )
			Sys.Array.Sort( array, 0, Count, new ComparisonComparer<T>( comparison ) );
	}

	public void Sort( Sys.Func<T, T, int> comparator, bool descending = false ) => Sort( LinqEx.DefaultExtractor, comparator, descending );

	public void Sort<E>( Sys.Func<T, E> extractor, Sys.Func<E, E, int> comparator, bool descending = false )
	{
		IComparer<T> comparer = new ExtractingComparer<T, E>( extractor, comparator, descending );
		Sort( comparer );
	}

	// [Sys.Obsolete( "Use AsArray() instead" )]
	// public readonly T[] ToArray()
	// {
	// 	if( Count == 0 )
	// 		return Sys.Array.Empty<T>();
	// 	T[] array = new T[Count];
	// 	Sys.Array.Copy( this.array, array, Count );
	// 	return array;
	// }

	public readonly Series<T> AsSeries() => new( array, 0, Count );
	public readonly IReadOnlyList<T> AsReadOnlyList() => AsSeries().AsReadOnlyList();

	public MutableArray<T> TrimExcess()
	{
		int threshold = (int)(array.Length * 0.9);
		if( Count < threshold )
			Capacity = Count;
		return this;
	}

	// public readonly bool TrueForAll( Sys.Predicate<T> match )
	// {
	// 	for( int i = 0; i < Count; i++ )
	// 		if( !match( array[i] ) )
	// 			return false;
	// 	return true;
	// }

	// public struct Enumerator : IEnumerator<T>
	// {
	// 	readonly MutableSeries<T> mutableSeries;
	// 	int index;
	// 	readonly int originalVersion;
	// 	public T Current { get; private set; } = default!;
	//
	// 	internal Enumerator( MutableSeries<T> mutableSeries )
	// 	{
	// 		this.mutableSeries = mutableSeries;
	// 		index = 0;
	// 		originalVersion = mutableSeries.version;
	// 	}
	//
	// 	public readonly void Dispose()
	// 	{ }
	//
	// 	public bool MoveNext()
	// 	{
	// 		Assert( originalVersion == mutableSeries.version );
	// 		if( (uint)index < (uint)mutableSeries.Count )
	// 		{
	// 			Current = mutableSeries.array[index];
	// 			index++;
	// 			return true;
	// 		}
	// 		return moveNextRare();
	// 	}
	//
	// 	bool moveNextRare()
	// 	{
	// 		index = mutableSeries.Count + 1;
	// 		Current = default!;
	// 		return false;
	// 	}
	//
	// 	readonly object? IEnumerator.Current
	// 	{
	// 		get
	// 		{
	// 			Assert( index != 0 && index != mutableSeries.Count + 1 );
	// 			return Current;
	// 		}
	// 	}
	//
	// 	void IEnumerator.Reset()
	// 	{
	// 		Assert( originalVersion == mutableSeries.version );
	// 		index = 0;
	// 		Current = default!;
	// 	}
	// }

	// class MyList : AbstractList<T>
	// {
	// 	public MyList() { }
	// 	public override IEnumerator<T> GetEnumerator() => throw new System.NotImplementedException();
	//
	// 	public override void Add( T item ) => throw new System.NotImplementedException();
	//
	// 	public override bool Contains( T item ) => throw new System.NotImplementedException();
	//
	// 	public override bool Remove( T item ) => throw new System.NotImplementedException();
	//
	// 	public override int Count { get; }
	// 	public override void Insert( int index, T item ) => throw new System.NotImplementedException();
	//
	// 	public override void RemoveAt( int index ) => throw new System.NotImplementedException();
	//
	// 	public override T this[ int index ] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
	// }
}
