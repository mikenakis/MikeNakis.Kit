namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using static MikeNakis.Kit.GlobalStatics;
using LegacyCollections = System.Collections;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

/// <summary>
/// A double-ended queue implements IList interface.
/// </summary>
/// <typeparam name="T">Type</typeparam>
/// <remarks>
/// O(1) Insert/RemoveAt for index 0 and n-1
/// Originally from https://github.com/ewgdg/circular-list/blob/main/CircularList/CircularList.cs
/// </remarks>
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class CircularList<T> : IList<T>
{
	T[] items;
	int headIndex;
	int tailIndex;
	public int Count { get; private set; }
	public bool IsReadOnly => false;

	public CircularList() : this( 16 )
	{
	}

	public CircularList( int initialCapacity )
	{
		Assert( initialCapacity < int.MaxValue );
		items = new T[initialCapacity];
		tailIndex = -1;
	}

	public T this[int index]
	{
		get
		{
			Assert( index >= 0 && index < Count );
			int internalIndex = getInternalIndex( index );
			return items[internalIndex];
		}
		set
		{
			Assert( index >= 0 && index < Count );
			int internalIndex = getInternalIndex( index );
			items[internalIndex] = value;
		}
	}

	public void Add( T item )
	{
		if( Count >= items.Length )
			expandCapacity();
		tailIndex = increaseInternalIndex( tailIndex );
		items[tailIndex] = item;
		++Count;
	}

	public void Clear()
	{
		Count = 0;
		headIndex = 0;
		tailIndex = -1;
	}

	public bool Contains( T? item )
	{
		foreach( T existingItem in this )
			if( EqualityComparer<T>.Default.Equals( existingItem, item ) )
				return true;
		return false;
	}

	public void CopyTo( T[] array, int arrayIndex )
	{
		if( Count == 0 )
			return;
		int copyCount = largestIndex - headIndex + 1;
		Sys.Array.Copy( items, headIndex, array, arrayIndex, copyCount );

		if( isWrapped( tailIndex ) )
			Sys.Array.Copy( items, 0, array, arrayIndex + copyCount, tailIndex + 1 );
	}

	public IEnumerator<T> GetEnumerator()
	{
		for( int i = 0, internalIndex = getInternalIndex( i ); i < Count; i++, internalIndex = increaseInternalIndex( internalIndex ) )
			yield return items[internalIndex];
	}

	LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();

	public int IndexOf( T item )
	{
		int i = 0;
		foreach( T existingItem in this )
		{
			if( EqualityComparer<T>.Default.Equals( existingItem, item ) )
				return i;
			i += 1;
		}
		return -1;
	}

	public void Insert( int index, T item )
	{
		Assert( index >= 0 && index < Count );

		if( Count >= items.Length )
			expandCapacity();

		if( index == 0 || index < (Count - 1) / 2 )
		{
			int internalIndex = getInternalIndex( index );
			int insertAtInternalIndex = decreaseInternalIndex( internalIndex );
			int newHead = decreaseInternalIndex( headIndex );

			if( index > 0 )
			{
				bool isInsertAtIndexWrapped = isWrapped( insertAtInternalIndex );
				items[newHead] = items[headIndex];
				int copyCount = (isInsertAtIndexWrapped ? items.Length - 1 : insertAtInternalIndex) - headIndex;
				if( copyCount > 0 )
					Sys.Array.Copy( items, headIndex + 1, items, headIndex, copyCount );

				if( isInsertAtIndexWrapped )
				{
					items[^1] = items[0];
					int copyCount2 = insertAtInternalIndex;
					if( copyCount2 > 0 )
						Sys.Array.Copy( items, 1, items, 0, copyCount2 );
				}
			}

			headIndex = newHead;
			items[insertAtInternalIndex] = item;
			++Count;
		}
		else
		{
			int internalIndex = getInternalIndex( index );
			if( isWrapped( internalIndex ) )
			{
				int copyCount = tailIndex - internalIndex + 1;
				if( copyCount > 0 )
					Sys.Array.Copy( items, internalIndex, items, internalIndex + 1, copyCount );
			}
			else
			{
				if( isWrapped( tailIndex ) )
					Sys.Array.Copy( items, 0, items, 1, tailIndex + 1 );

				int largestIndex = this.largestIndex;
				items[increaseInternalIndex( largestIndex )] = items[largestIndex];
				int copyCount = largestIndex - internalIndex;
				if( copyCount > 0 )
					Sys.Array.Copy( items, internalIndex, items, internalIndex + 1, copyCount );
			}

			tailIndex = increaseInternalIndex( tailIndex );
			items[internalIndex] = item;
			++Count;
		}
	}

	public bool Remove( T item )
	{
		int index = IndexOf( item );
		if( index < 0 )
			return false;
		RemoveAt( index );
		return true;
	}

	public void RemoveAt( int index )
	{
		Assert( index >= 0 && index < Count );

		if( index == 0 || index < (Count - 1) / 2 )
		{
			int internalIndex = getInternalIndex( index );
			if( !isWrapped( internalIndex ) )
			{
				int copyCount = internalIndex - headIndex;
				if( copyCount > 0 )
					Sys.Array.Copy( items, headIndex, items, headIndex + 1, copyCount );
			}
			else
			{
				int copyCount = internalIndex;
				if( copyCount > 0 )
					Sys.Array.Copy( items, 0, items, 1, copyCount );
				items[0] = items[^1];
				int copyCount2 = items.Length - 1 - headIndex;
				if( copyCount2 > 0 )
					Sys.Array.Copy( items, headIndex, items, headIndex + 1, copyCount2 );
			}

			Count--;
			headIndex = increaseInternalIndex( headIndex );
		}
		else
		{
			int internalIndex = getInternalIndex( index );
			if( isWrapped( internalIndex ) )
			{
				//Array.Copy can handle overlap
				int copyCount = tailIndex - internalIndex;
				if( copyCount > 0 )
					Sys.Array.Copy( items, internalIndex + 1, items, internalIndex, copyCount );
			}
			else
			{
				int copyCount = largestIndex - internalIndex;
				Sys.Array.Copy( items, internalIndex + 1, items, internalIndex, copyCount );
				if( isWrapped( tailIndex ) )
				{
					items[^1] = items[0];
					int copyCount2 = tailIndex;
					if( copyCount2 > 0 )
						Sys.Array.Copy( items, 1, items, 0, tailIndex );
				}
			}

			Count--;
			tailIndex = decreaseInternalIndex( tailIndex );
		}
	}

	public override string ToString() => $"{Count} items";

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	int largestIndex => isWrapped( tailIndex ) ? items.Length - 1 : tailIndex;

	void expandCapacity()
	{
		int capacity = items.Length * 2;
		if( (uint)capacity >= int.MaxValue )
			capacity = int.MaxValue - 1;
		if( capacity == items.Length )
			throw new AssertionFailureException(); //maximum size limit reached.
		T[] newItems = new T[capacity];
		CopyTo( newItems, 0 );
		items = newItems;
		headIndex = 0;
		tailIndex = Count - 1;
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	bool isWrapped( int internalIndex ) => internalIndex < headIndex;

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	int correctInternalIndex( int internalIndex )
	{
		if( internalIndex >= items.Length || internalIndex < 0 )
			internalIndex %= items.Length;
		if( internalIndex < 0 )
			internalIndex += items.Length;
		return internalIndex;
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	int getInternalIndex( int index ) => correctInternalIndex( index + headIndex );

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	int increaseInternalIndex( int internalIndex ) => correctInternalIndex( internalIndex + 1 );

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	int decreaseInternalIndex( int internalIndex ) => correctInternalIndex( internalIndex - 1 );
}
