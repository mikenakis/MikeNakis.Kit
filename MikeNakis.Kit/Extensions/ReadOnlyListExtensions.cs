namespace MikeNakis.Kit.Extensions;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit;
using MikeNakis.Kit.Extensions;
using LegacyCollections = System.Collections;
using Sys = System;
using SysDiag = System.Diagnostics;

public static class ReadOnlyListExtensions
{
	[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
	[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
	sealed class SelectList<T, U> : IReadOnlyList<U>
	{
		readonly IReadOnlyList<T> list;
		readonly Sys.Func<T, int, U> converter;

		public SelectList( IReadOnlyList<T> list, Sys.Func<T, int, U> converter )
		{
			this.list = list;
			this.converter = converter;
		}

		public IEnumerator<U> GetEnumerator() => ((IEnumerable<T>)list).Select( converter ).GetEnumerator();
		LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();
		public int Count => list.Count;
		public U this[int index] => converter.Invoke( list[index], index );
	}

	[SysDiag.DebuggerDisplay( "Count = {" + nameof( Count ) + "}" )]
	[SysDiag.DebuggerTypeProxy( typeof( EnumerableDebugView ) )]
	sealed class SelectManyList<T, U> : IReadOnlyList<U>
	{
		readonly IReadOnlyList<T> list;
		readonly Sys.Func<T, IReadOnlyList<U>> converter;

		public SelectManyList( IReadOnlyList<T> list, Sys.Func<T, IReadOnlyList<U>> converter )
		{
			this.list = list;
			this.converter = converter;
		}

		public IEnumerator<U> GetEnumerator() => ((IEnumerable<T>)list).SelectMany( converter ).GetEnumerator();
		LegacyCollections.IEnumerator LegacyCollections.IEnumerable.GetEnumerator() => GetEnumerator();
		public int Count => list.Select( item => converter( item ) ).Sum( list => list.Count );
		public U this[int index] => get( index );

		U get( int index )
		{
			foreach( T item in list )
			{
				IReadOnlyList<U> targetList = converter.Invoke( item );
				int count = targetList.Count;
				if( index < count )
					return targetList[index];
				index -= count;
			}
#pragma warning disable CA2201 // Do not raise reserved exception types
			throw new Sys.IndexOutOfRangeException();
#pragma warning restore CA2201 // Do not raise reserved exception types
		}
	}

	public static IReadOnlyList<U> Select<T, U>( this IReadOnlyList<T> self, Sys.Func<T, U> converter ) => new SelectList<T, U>( self, ( t, _ ) => converter.Invoke( t ) );
	public static IReadOnlyList<U> Select<T, U>( this IReadOnlyList<T> self, Sys.Func<T, int, U> converter ) => new SelectList<T, U>( self, converter );
	public static IReadOnlyList<U> SelectMany<T, U>( this IReadOnlyList<T> self, Sys.Func<T, IReadOnlyList<U>> converter ) => new SelectManyList<T, U>( self, converter );
	public static int BinarySearch<E, T>( this IReadOnlyList<E> self, T item, Sys.Func<E, T> extractor, Sys.Comparison<T> comparison, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, extractor, comparison, skipDuplicates );
	public static int BinarySearch<E>( this IReadOnlyList<E> self, E item, Sys.Comparison<E> comparison, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, comparison, skipDuplicates );
	public static int BinarySearch<T>( this IReadOnlyList<T> self, T item, IComparer<T> comparer, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, comparer, skipDuplicates );
	public static int BinarySearch<E, T>( this IReadOnlyList<E> self, T item, Sys.Func<E, T> extractor, bool skipDuplicates = false ) where T : Sys.IComparable<T> => KitHelpers.BinarySearch( self, item, extractor, Comparer<T>.Default, skipDuplicates );
	public static int BinarySearch<E>( this IReadOnlyList<E> self, E item, bool skipDuplicates = false ) where E : Sys.IComparable<E> => KitHelpers.BinarySearch( self, item, Comparer<E>.Default, skipDuplicates );
	public static int IndexOf<E>( this IReadOnlyList<E> self, E elementToFind )
	{
		int i = 0;
		foreach( E element in self )
		{
			if( DotNetHelpers.Equal( element, elementToFind ) )
				return i;
			i++;
		}
		return -1;
	}

	public static T? LastOrDefault<T>( this IReadOnlyList<T> self ) => self.Count == 0 ? default : self[^1];
}
