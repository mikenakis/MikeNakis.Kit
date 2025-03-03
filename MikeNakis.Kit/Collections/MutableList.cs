namespace MikeNakis.Kit.Collections;

using System.Collections;
using System.Collections.Generic;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public sealed class MutableList<T> : IEnumerable<T>
{
	const int defaultCapacity = 4;

	T[] array;
	int version;
	public bool IsFrozen { get; private set; }
	public int Capacity => array.Length;
	IReadOnlyList<T>? lazyAsReadOnlyList;
	public IReadOnlyList<T> AsReadOnlyList => lazyAsReadOnlyList ??= new ReadOnlyList( this );
	public IEnumerable<T> AsEnumerable => AsReadOnlyList;

	public int Count { get; private set; }

	public MutableList()
	{
		array = Sys.Array.Empty<T>();
	}

	public MutableList( int capacity )
	{
		Assert( capacity >= 0 );
		array = capacity == 0 ? Sys.Array.Empty<T>() : new T[capacity];
	}

	public MutableList( IEnumerable<T> enumerable )
	{
		if( enumerable is IReadOnlyCollection<T> readOnlyCollection )
		{
			int count = readOnlyCollection.Count;
			if( count == 0 )
				array = Sys.Array.Empty<T>();
			else
			{
				array = new T[count];
				int index = 0;
				foreach( T item in readOnlyCollection )
					array[index++] = item;
				Count = count;
			}
		}
		else if( enumerable is ICollection<T> collection )
		{
			int count = collection.Count;
			if( count == 0 )
				array = Sys.Array.Empty<T>();
			else
			{
				array = new T[count];
				collection.CopyTo( array, 0 );
				Count = count;
			}
		}
		else
		{
			array = Sys.Array.Empty<T>();
			foreach( T item in enumerable )
				Add( item );
		}
	}

	[Sys.Obsolete]
	public override bool Equals( object? other )
	{
		return other switch
		{
			MutableList<T> mutableList => Equals( mutableList ),
			ArrayWrapper<T> arrayWrapper => Equals( arrayWrapper ),
			IReadOnlyList<T> readOnlyList => Equals( readOnlyList ),
			IEnumerable<T> enumerable => Equals( enumerable ),
			_ => false
		};
	}

	public bool Equals( MutableList<T> other )
	{
		int size = Count;
		if( size != other.Count )
			return false;
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for( int i = 0; i < size; i++ )
			if( !equalityComparer.Equals( array[i], other.array[i] ) )
				return false;
		return true;
	}

	public bool Equals( ArrayWrapper<T> other )
	{
		int size = Count;
		if( size != other.Count )
			return false;
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for( int i = 0; i < size; i++ )
			if( !equalityComparer.Equals( array[i], other[i] ) )
				return false;
		return true;
	}

	public bool Equals( IReadOnlyList<T> other )
	{
		// ReSharper disable once SuspiciousTypeConversion.Global
		Assert( other is not Sys.Array );
		int size = Count;
		if( size != other.Count )
			return false;
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for( int i = 0; i < size; i++ )
		{
			T rightItem = other[i];
			if( !equalityComparer.Equals( array[i], rightItem ) )
				return false;
		}
		return true;
	}

	public bool Equals( IEnumerable<T> other )
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		if( False )
		{
			Enumerator selfEnumerator = GetEnumerator();
			IEnumerator<T> otherEnumerator = other.GetEnumerator();
			try
			{
				while( true )
				{
					bool selfHasNext = selfEnumerator.MoveNext();
					bool otherHasNext = otherEnumerator.MoveNext();
					if( !selfHasNext && !otherHasNext )
						break;
					if( selfHasNext || otherHasNext )
						return false;
					T selfItem = selfEnumerator.Current;
					T otherItem = otherEnumerator.Current;
					if( !equalityComparer.Equals( selfItem, otherItem ) )
						return false;
				}
			}
			finally
			{
				otherEnumerator.Dispose();
				selfEnumerator.Dispose();
			}
		}
		else if( False )
		{
			IEnumerator<T> otherEnumerator = other.GetEnumerator();
			try
			{
				foreach( T selfItem in this )
				{
					if( !otherEnumerator.MoveNext() )
						return false;
					T otherItem = otherEnumerator.Current;
					if( !equalityComparer.Equals( selfItem, otherItem ) )
						return false;
				}
				if( otherEnumerator.MoveNext() )
					return false;
			}
			finally
			{
				otherEnumerator.Dispose();
			}
		}
		else
		{
			IEnumerator<T> otherEnumerator = other.GetEnumerator();
			try
			{
				for( int i = 0; i < Count; i++ )
				{
					if( !otherEnumerator.MoveNext() )
						return false;
					T otherItem = otherEnumerator.Current;
					if( !equalityComparer.Equals( array[i], otherItem ) )
						return false;
				}
				if( otherEnumerator.MoveNext() )
					return false;
			}
			finally
			{
				otherEnumerator.Dispose();
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		Sys.HashCode hashCodeBuilder = new();
		// ReSharper disable once NonReadOnlyMemberInGetHashCode
		foreach( T item in array )
			hashCodeBuilder.Add( item );
		return hashCodeBuilder.ToHashCode();
	}

	public void SetCapacity( int capacity )
	{
		Assert( capacity >= Count );
		if( capacity == array.Length )
			return;
		if( capacity <= 0 )
		{
			Assert( false ); //I do not think this can ever happen.
			array = Sys.Array.Empty<T>();
		}
		else
		{
			T[] newItems = new T[capacity];
			if( Count > 0 )
				Sys.Array.Copy( array, newItems, Count );
			array = newItems;
		}
	}

	public T this[int index]
	{
		get
		{
			Assert( (uint)index < (uint)Count );
			return array[index];
		}
		set
		{
			Assert( !IsFrozen );
			Assert( (uint)index < (uint)Count );
			array[index] = value;
			version++;
		}
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public void Add( T item )
	{
		Assert( !IsFrozen );
		version++;
		T[] array = this.array;
		int size = Count;
		if( (uint)size < (uint)array.Length )
		{
			Count = size + 1;
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
		Count = size + 1;
		array[size] = item;
	}

	public void AddRange( IEnumerable<T> enumerable )
	{
		if( enumerable is IReadOnlyCollection<T> readOnlyCollection )
		{
			int count = readOnlyCollection.Count;
			if( count > 0 )
			{
				if( array.Length - Count < count )
					grow( checked(Count + count) );
				readOnlyCollection.CopyTo( array, Count );
				Count += count;
				version++;
			}
		}
		if( enumerable is ICollection<T> collection )
		{
			int count = collection.Count;
			if( count > 0 )
			{
				if( array.Length - Count < count )
					grow( checked(Count + count) );
				collection.CopyTo( array, Count );
				Count += count;
				version++;
			}
		}
		else
		{
			foreach( T item in enumerable )
				Add( item );
		}
	}

	// public int BinarySearch( int index, int count, T item, IComparer<T>? comparer )
	// {
	// 	Assert( index >= 0 && count >= 0 && size - index >= count );
	// 	return Sys.Array.BinarySearch( array, index, count, item, comparer );
	// }

	// public int BinarySearch( T item ) => BinarySearch( 0, size, item, null );

	// public int BinarySearch( T item, IComparer<T>? comparer ) => BinarySearch( 0, size, item, comparer );

	public void Clear()
	{
		Assert( !IsFrozen );
		version++;
		if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
		{
			int size = Count;
			Count = 0;
			if( size > 0 )
				Sys.Array.Clear( array, 0, size ); // Clear the elements so that the gc can reclaim the references.
		}
		else
			Count = 0;
	}

	public bool Contains( T item ) => Count != 0 && IndexOf( item ) >= 0;

	// public void CopyTo( T[] array ) => CopyTo( array, 0 );

	// public void CopyTo( int index, T[] array, int arrayIndex, int count )
	// {
	// 	Assert( size - index >= count );
	// 	Sys.Array.Copy( this.array, index, array, arrayIndex, count );
	// }

	public void CopyTo( T[] array, int arrayIndex )
	{
		Sys.Array.Copy( this.array, 0, array, arrayIndex, Count );
	}

	public int EnsureCapacity( int capacity )
	{
		Assert( !IsFrozen );
		Assert( capacity >= 0 );
		if( array.Length < capacity )
			grow( capacity );
		return array.Length;
	}

	void grow( int capacity )
	{
		Assert( array.Length < capacity );

		int newCapacity = array.Length == 0 ? defaultCapacity : 2 * array.Length;

		// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
		// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
		if( (uint)newCapacity > Sys.Array.MaxLength )
			newCapacity = Sys.Array.MaxLength;

		// If the computed capacity is still less than specified, set to the original argument.
		// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
		if( newCapacity < capacity )
			newCapacity = capacity;

		SetCapacity( newCapacity );
	}

	// public bool Exists( Sys.Predicate<T> match ) => FindIndex( match ) != -1;
	//
	// public T? Find( Sys.Predicate<T> match )
	// {
	// 	for( int i = 0; i < size; i++ )
	// 		if( match( array[i] ) )
	// 			return array[i];
	// 	return default;
	// }

	public int FindIndex( Sys.Predicate<T> match ) => FindIndex( 0, Count, match );
	public int FindIndex( int startIndex, Sys.Predicate<T> match ) => FindIndex( startIndex, Count - startIndex, match );

	public int FindIndex( int startIndex, int count, Sys.Predicate<T> match )
	{
		Assert( (uint)startIndex <= (uint)Count );
		Assert( count >= 0 && startIndex <= Count - count );
		int endIndex = startIndex + count;
		for( int i = startIndex; i < endIndex; i++ )
		{
			if( match( array[i] ) )
				return i;
		}
		return -1;
	}

	// public T? FindLast( Sys.Predicate<T> match )
	// {
	// 	for( int i = size - 1; i >= 0; i-- )
	// 		if( match( array[i] ) )
	// 			return array[i];
	// 	return default;
	// }

	// public int FindLastIndex( Sys.Predicate<T> match ) => FindLastIndex( size - 1, size, match );
	//
	// public int FindLastIndex( int startIndex, Sys.Predicate<T> match ) => FindLastIndex( startIndex, startIndex + 1, match );
	//
	// public int FindLastIndex( int startIndex, int count, Sys.Predicate<T> match )
	// {
	// 	Assert( size == 0 ? startIndex == -1 : (uint)startIndex < (uint)size );
	// 	Assert( count >= 0 && startIndex - count + 1 >= 0 );
	//
	// 	int endIndex = startIndex - count;
	// 	for( int i = startIndex; i > endIndex; i-- )
	// 		if( match( array[i] ) )
	// 			return i;
	// 	return -1;
	// }

	// public void ForEach( Sys.Action<T> action )
	// {
	// 	int version = this.version;
	// 	for( int i = 0; i < size; i++ )
	// 	{
	// 		if( version != this.version )
	// 			break;
	// 		action( array[i] );
	// 	}
	// 	if( version != this.version )
	// 		throw new Sys.InvalidOperationException();
	// }

	public Enumerator GetEnumerator() => new( this );
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public int IndexOf( T item ) => Sys.Array.IndexOf( array, item, 0, Count );

	// public int IndexOf( T item, int index )
	// {
	// 	Assert( index <= size );
	// 	return Sys.Array.IndexOf( array, item, index, size - index );
	// }
	//
	// public int IndexOf( T item, int index, int count )
	// {
	// 	Assert( index <= size );
	// 	Assert( count >= 0 && index <= size - count );
	// 	return Sys.Array.IndexOf( array, item, index, count );
	// }

	public void Insert( int index, T item )
	{
		Assert( !IsFrozen );
		Assert( (uint)index <= (uint)Count ); // Note that insertions at the end are legal.
		if( Count == array.Length )
			grow( Count + 1 );
		if( index < Count )
			Sys.Array.Copy( array, index, array, index + 1, Count - index );
		array[index] = item;
		Count++;
		version++;
	}

	// public void InsertRange( int index, IEnumerable<T> enumerable )
	// {
	// 	Assert( (uint)index <= (uint)size );
	// 	switch( enumerable )
	// 	{
	// 		case MutableList<T> mutableList:
	// 		{
	// 			int count = mutableList.Count;
	// 			if( count > 0 )
	// 			{
	// 				if( array.Length - size < count )
	// 					grow( checked(size + count) );
	// 				if( index < size )
	// 					Sys.Array.Copy( array, index, array, index + count, size - index );
	// 				if( ReferenceEquals( array, mutableList.array ) )
	// 				{
	// 					Sys.Array.Copy( array, 0, array, index, index ); // Copy first part of array to insert location
	// 					Sys.Array.Copy( array, index + count, array, index * 2, size - index ); // Copy last part of array back to inserted location
	// 				}
	// 				else
	// 					mutableList.CopyTo( array, index );
	// 				size += count;
	// 				version++;
	// 			}
	// 			break;
	// 		}
	// 		case ArrayWrapper<T> series:
	// 		{
	// 			int count = series.Count;
	// 			if( count > 0 )
	// 			{
	// 				if( array.Length - size < count )
	// 					grow( checked(size + count) );
	// 				if( index < size )
	// 					Sys.Array.Copy( array, index, array, index + count, size - index );
	// 				series.CopyTo( array, index );
	// 				size += count;
	// 				version++;
	// 			}
	// 			break;
	// 		}
	// 		case ICollection<T> collection:
	// 		{
	// 			int count = collection.Count;
	// 			if( count > 0 )
	// 			{
	// 				if( array.Length - size < count )
	// 					grow( checked(size + count) );
	// 				if( index < size )
	// 					Sys.Array.Copy( array, index, array, index + count, size - index );
	// 				collection.CopyTo( array, index );
	// 				size += count;
	// 				version++;
	// 			}
	// 			break;
	// 		}
	// 		default:
	// 		{
	// 			foreach( T item in enumerable )
	// 				Insert( index++, item );
	// 			break;
	// 		}
	// 	}
	// }

	// public int LastIndexOf( T item ) => size == 0 ? -1 : LastIndexOf( item, size - 1, size ); //TODO: conditional operator is probably unnecessary here
	//
	// public int LastIndexOf( T item, int index )
	// {
	// 	Assert( index < size );
	// 	return LastIndexOf( item, index, index + 1 );
	// }

	// public int LastIndexOf( T item, int index, int count )
	// {
	// 	Assert( size == 0 || index >= 0 );
	// 	Assert( size == 0 || count >= 0 );
	// 	if( size == 0 ) // Special case for empty list
	// 		return -1;
	// 	Assert( index < size );
	// 	Assert( count <= index + 1 );
	// 	return Sys.Array.LastIndexOf( array, item, index, count );
	// }

	public bool Remove( T item )
	{
		Assert( !IsFrozen );
		int index = IndexOf( item );
		if( index >= 0 )
		{
			RemoveAt( index );
			return true;
		}
		return false;
	}

	// public int RemoveAll( Sys.Predicate<T> match )
	// {
	// 	int freeIndex = 0; // the first free slot in items array
	// 	while( freeIndex < size && !match( array[freeIndex] ) ) // Find the first item which needs to be removed.
	// 		freeIndex++;
	// 	if( freeIndex >= size )
	// 		return 0;
	// 	int current = freeIndex + 1;
	// 	while( current < size )
	// 	{
	// 		while( current < size && match( array[current] ) ) // Find the first item which needs to be kept.
	// 			current++;
	// 		if( current < size )
	// 			array[freeIndex++] = array[current++]; // copy item to the free slot.
	// 	}
	// 	if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
	// 		Sys.Array.Clear( array, freeIndex, size - freeIndex ); // Clear the elements so that the gc can reclaim the references.
	// 	int result = size - freeIndex;
	// 	size = freeIndex;
	// 	version++;
	// 	return result;
	// }

	public void RemoveAt( int index )
	{
		Assert( !IsFrozen );
		Assert( (uint)index < (uint)Count );
		Count--;
		if( index < Count )
			Sys.Array.Copy( array, index + 1, array, index, Count - index );
		if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
			array[Count] = default!;
		version++;
	}

	public void RemoveRange( int index, int count )
	{
		Assert( !IsFrozen );
		Assert( index >= 0 );
		Assert( count >= 0 );
		Assert( Count - index >= count );
		if( count > 0 )
		{
			Count -= count;
			if( index < Count )
				Sys.Array.Copy( array, index + count, array, index, Count - index );
			version++;
			if( SysCompiler.RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
				Sys.Array.Clear( array, Count, count );
		}
	}

	public void Reverse() => Reverse( 0, Count );

	public void Reverse( int index, int count )
	{
		Assert( !IsFrozen );
		Assert( index >= 0 );
		Assert( count >= 0 );
		Assert( Count - index >= count );
		if( count > 1 )
			Sys.Array.Reverse( array, index, count );
		version++;
	}

	public void Sort() => Sort( 0, Count, null );

	public void Sort( IComparer<T>? comparer ) => Sort( 0, Count, comparer );

	public void Sort( int index, int count, IComparer<T>? comparer )
	{
		Assert( !IsFrozen );
		Assert( index >= 0 );
		Assert( count >= 0 );
		Assert( Count - index >= count );
		if( count > 1 )
			Sys.Array.Sort( array, index, count, comparer );
		version++;
	}

	public void Sort( Sys.Comparison<T> comparison )
	{
		IComparer<T> comparer = new ComparisonComparer<T>( comparison );
		Sort( comparer );
	}

	public void Sort( Sys.Func<T, T, int> comparator, bool descending = false ) => Sort( LinqEx.DefaultExtractor, comparator, descending );

	public void Sort<E>( Sys.Func<T, E> extractor, Sys.Func<E, E, int> comparator, bool descending = false )
	{
		IComparer<T> comparer = new ExtractingComparer<T, E>( extractor, comparator, descending );
		Sort( comparer );
	}

	[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
	[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
	sealed class ReadOnlyList : AbstractReadOnlyList<T>
	{
		readonly MutableList<T> list;

		internal ReadOnlyList( MutableList<T> list )
		{
			this.list = list;
		}

		public override IEnumerator<T> GetEnumerator() => list.GetEnumerator();
		public override int Count => list.Count;
		public override T this[int index] => list[index];
	}

	public ArrayWrapper<T> Freeze()
	{
		Assert( !IsFrozen );
		IsFrozen = true;
		return new ArrayWrapper<T>( array, 0, Count );
	}

	public void TrimExcess()
	{
		Assert( !IsFrozen );
		int threshold = (int)(array.Length * 0.9);
		if( Count < threshold )
			SetCapacity( Count );
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		readonly MutableList<T> list;
		int index;
		readonly int originalVersion;
		T? current;

		internal Enumerator( MutableList<T> list )
		{
			this.list = list;
			index = 0;
			originalVersion = list.version;
			current = default;
		}

		public readonly void Dispose()
		{ }

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
		public bool MoveNext()
		{
			Assert( originalVersion == list.version );
			if( (uint)index < (uint)list.Count )
			{
				current = list.array[index];
				index++;
				return true;
			}
			return moveNextRare();
		}

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.NoInlining )]
		bool moveNextRare()
		{
			index = list.Count + 1;
			current = default;
			return false;
		}

		public readonly T Current
		{
			get
			{
				Assert( index != 0 && index != list.Count + 1 );
				return current!;
			}
		}

		readonly object? IEnumerator.Current => Current;

		void IEnumerator.Reset()
		{
			Assert( originalVersion == list.version );
			index = 0;
			current = default;
		}
	}

	public void DoRemove( T element )
	{
		bool ok = Remove( element );
		Assert( ok );
	}

	public bool IsEmpty() => Count == 0;

	public void RemoveLast() => RemoveAt( Count - 1 );

	public T Last() => this[Count - 1];

	public T ExtractAt( int index )
	{
		T result = this[index];
		RemoveAt( index );
		return result;
	}

	public void Move( int oldIndex, int newIndex )
	{
		if( oldIndex == newIndex )
			return;
		T item = this[oldIndex];
		RemoveAt( oldIndex );
		Insert( newIndex, item );
	}
}
