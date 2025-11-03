namespace MikeNakis.Kit.Collections;

using System.Collections.Generic;
using MikeNakis.Kit;
using LegacyCollections = System.Collections;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

public static class ArrayWrapper
{
	public static ArrayWrapper<T> Of<T>() => new();
	public static ArrayWrapper<T> Of<T>( T item ) => new( new[] { item } );
	public static ArrayWrapper<T> Of<T>( T item1, T item2 ) => new( new[] { item1, item2 } );
	public static ArrayWrapper<T> Of<T>( T item1, T item2, T item3 ) => new( new[] { item1, item2, item3 } );
	public static ArrayWrapper<T> Of<T>( params T[] items ) => new( items );
}

///<summary>An immutable wrapper of <c>T[]</c> which properly implements <see cref="object.Equals(object?)"/> and <see cref="object.GetHashCode()"/>.</summary>
[SysDiag.DebuggerDisplay( "Length = {" + nameof( size ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public readonly struct ArrayWrapper<T> : IEnumerable<T>, Sys.IEquatable<ArrayWrapper<T>>, IReadOnlyList<T>
{
	public static bool operator ==( ArrayWrapper<T> left, ArrayWrapper<T> right ) => left.Equals( right );
	public static bool operator !=( ArrayWrapper<T> left, ArrayWrapper<T> right ) => !left.Equals( right );
	public static ArrayWrapper<T> Empty => new();

	readonly T[] array;
	readonly int start;
	readonly int size;

	public ArrayWrapper( T[] array )
			: this( array, 0, array.Length )
	{ }

	public ArrayWrapper( IReadOnlyList<T> readOnlyList )
			: this( new T[readOnlyList.Count], 0, readOnlyList.Count )
	{
		readOnlyList.CopyTo( array, 0 );
	}

	public ArrayWrapper( T[] array, int start, int size )
	{
		Assert( (uint)start <= (uint)array.Length && (uint)size <= (uint)(array.Length - start) );
		this.array = array;
		this.start = start;
		this.size = size;
	}

	public bool IsEmpty() => Count == 0;
	public Enumerator GetEnumerator() => new( array, start, size );
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();
	public Sys.ReadOnlySpan<T> AsSpan => new( array, start, size );

	public int Count => size;

	public T this[int index]
	{
		get
		{
			Assert( index >= 0 && index < size );
			return array[start + index];
		}
	}

	public void CopyTo( T[] array ) => CopyTo( array, 0 );

	public void CopyTo( int index, T[] array, int arrayIndex, int count )
	{
		Assert( size - index >= count );
		Sys.Array.Copy( this.array, index, array, arrayIndex, count );
	}

	public void CopyTo( T[] array, int arrayIndex )
	{
		Sys.Array.Copy( this.array, 0, array, arrayIndex, size );
	}

	public override bool Equals( object? other )
	{
		return other switch
		{
			ArrayWrapper<T> arrayWrapper => Equals( arrayWrapper ),
			IEnumerable<T> enumerable => Equals( enumerable ),
			_ => false
		};
	}

	public bool Equals( IEnumerable<T> other )
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		int i = 0;
		foreach( T element in other )
		{
			if( i >= size )
				return false;
			if( !equalityComparer.Equals( array[i], element ) )
				return false;
			i++;
		}
		return i == size;
	}

	public bool Equals( ArrayWrapper<T> other )
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		if( size != other.size )
			return false;
		for( int i = 0; i < size; i++ )
			if( !equalityComparer.Equals( array[i], other.array[i] ) )
				return false;
		return true;
	}

	public override int GetHashCode()
	{
		Sys.HashCode hashCode = new();
		foreach( T item in this )
			hashCode.Add( item?.GetHashCode() ?? 0 );
		return hashCode.ToHashCode();
	}

	public struct Enumerator : IEnumerator<T>, LegacyCollections.IEnumerator
	{
		readonly T[] array;
		readonly int start;
		readonly int length;
		int index;
		T current = default!;

		internal Enumerator( T[] array, int start, int length )
		{
			this.array = array;
			this.start = start;
			this.length = length;
			index = 0;
		}

		public readonly void Dispose()
		{ }

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
		public bool MoveNext()
		{
			if( (uint)index < (uint)length )
			{
				current = array[start + index];
				index++;
				return true;
			}
			return moveNextRare();
		}

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.NoInlining )]
		bool moveNextRare()
		{
			index = length + 1;
			current = default!;
			return false;
		}

		public readonly T Current => current!;

		readonly object? LegacyCollections.IEnumerator.Current => current!;

		void LegacyCollections.IEnumerator.Reset()
		{
			index = 0;
			current = default!;
		}
	}
}
