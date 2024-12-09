namespace MikeNakis.Kit.Collections;

using System.Collections;
using System.Collections.Generic;
using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

public static class Series
{
	public static Series<T> Of<T>() => new();
	public static Series<T> Of<T>( T item ) => new( new[] { item } );
	public static Series<T> Of<T>( T item1, T item2 ) => new( new[] { item1, item2 } );
	public static Series<T> Of<T>( T item1, T item2, T item3 ) => new( new[] { item1, item2, item3 } );
	public static Series<T> Of<T>( params T[] items ) => new( items );
}

///<summary>An immutable wrapper of <c>T[]</c> which properly implements <see cref="object.Equals(object?)"/> and <see cref="object.GetHashCode()"/>.</summary>
[SysDiag.DebuggerDisplay( "Length = {" + nameof( size ) + "}" )]
[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
public readonly struct Series<T> : IEnumerable<T>, Sys.IEquatable<Series<T>>
{
	public static bool operator ==( Series<T> left, Series<T> right ) => left.Equals( right );
	public static bool operator !=( Series<T> left, Series<T> right ) => !left.Equals( right );
	public static Series<T> Empty => new();

	readonly T[] array;
	readonly int start;
	readonly int size;

	public Series( T[] array )
			: this( array, 0, array.Length )
	{ }

	public Series( IReadOnlyList<T> readOnlyList )
			: this( new T[readOnlyList.Count], 0, readOnlyList.Count )
	{
		readOnlyList.CopyTo( array, 0 );
	}

	public Series( T[] array, int start, int size )
	{
		Assert( (uint)start <= (uint)array.Length && (uint)size <= (uint)(array.Length - start) );
		this.array = array;
		this.start = start;
		this.size = size;
	}

	public bool IsEmpty() => Count == 0;
	public IReadOnlyList<T> AsReadOnlyList() => new MyReadOnlyList( array, start, size );
	public Enumerator GetEnumerator() => new( array, start, size );
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
			Series<T> series => Equals( series ),
			IEnumerable<T> enumerable => Equals( enumerable ),
			_ => false
		};
	}

	public bool Equals( Series<T> other )
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

	public struct Enumerator : IEnumerator<T>, IEnumerator
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

		readonly object? IEnumerator.Current
		{
			get
			{
				Assert( index != 0 && index != length + 1 );
				return current;
			}
		}

		void IEnumerator.Reset()
		{
			index = 0;
			current = default!;
		}
	}

	//TODO: revise the usefulness of this class
	[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
	sealed class MyReadOnlyList : AbstractReadOnlyList<T>
	{
		readonly T[] array;
		readonly int start;
		readonly int size;

		public MyReadOnlyList( T[] array, int start, int size )
		{
			this.array = array;
			this.start = start;
			this.size = size;
		}

		public override IEnumerator<T> GetEnumerator() => new Enumerator( array, start, size );
		public override int Count => size;

		public override T this[int index]
		{
			get
			{
				Assert( (uint)index < (uint)size );
				return array[start + index];
			}
		}

		public override bool Equals( object? other )
		{
			return other switch
			{
				Series<T> series => equals( series ),
				IEnumerable<T> enumerable => Equals( enumerable ),
				_ => false
			};
		}

		bool equals( Series<T> other )
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
	}
}
