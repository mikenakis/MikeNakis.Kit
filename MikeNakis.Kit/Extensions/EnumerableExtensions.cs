namespace MikeNakis.Kit.Extensions;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using LegacyCollections = System.Collections;
using Sys = System;
using SysText = System.Text;

public static class EnumerableExtensions
{
	public static string MakeString( this IEnumerable<string> self ) => self.MakeString( "", "", "", "" );

	public static string MakeString( this IEnumerable<string> self, string delimiter ) => self.MakeString( "", delimiter, "", "" );

	public static string MakeString( this IEnumerable<string> self, string prefix, string delimiter, string suffix, string ifEmpty )
	{
		SysText.StringBuilder stringBuilder = new();
		TextConsumer textConsumer = new StringBuilderTextConsumer( stringBuilder );
		Sys.Action<string> elementConsumer = s => stringBuilder.Append( s );
		self.MakeString( elementConsumer, prefix, delimiter, suffix, ifEmpty, textConsumer );
		return stringBuilder.ToString();
	}

	public static void MakeString<T>( this IEnumerable<T> self, Sys.Action<T> elementConsumer, string prefix, string delimiter, string suffix, string ifEmpty, TextConsumer textConsumer )
	{
		bool first = true;
		foreach( T element in self )
		{
			if( first )
			{
				textConsumer.Write( prefix );
				first = false;
			}
			else
				textConsumer.Write( delimiter );
			elementConsumer.Invoke( element );
		}
		textConsumer.Write( first ? ifEmpty : suffix );
	}

	public static IEnumerable<T> Except<T>( this IEnumerable<T> enumerable, T item ) => enumerable.Where( e => !Equals( e, item ) );
	public static IEnumerable<T> Except<T>( this IEnumerable<T> enumerable, params T[] items ) => enumerable.Except( EnumerableOf( items ) );

	public static ListDictionary<K, V> ToListDictionary<T, K, V>( this IEnumerable<T> self, Sys.Func<T, K> keySelector, Sys.Func<T, V> elementSelector ) where K : notnull
	{
		if( self is ICollection<T> collection )
		{
			if( collection is T[] array )
				return fromArray( array );
			if( collection is List<T> list )
				return fromList( list );
		}
		return fromEnumerable( self );

		ListDictionary<K, V> fromEnumerable( IEnumerable<T> enumerable )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in enumerable )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}

		ListDictionary<K, V> fromArray( T[] array )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in array )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}

		ListDictionary<K, V> fromList( List<T> list )
		{
			ListDictionary<K, V> d = new();
			foreach( T element in list )
				d.Add( keySelector( element ), elementSelector( element ) );
			return d;
		}
	}

	public static X DefaultExtractor<X>( X a ) => a;
	public static int DefaultComparator<T>( T a, T b ) where T : Sys.IComparable<T> => a.CompareTo( b );

	public static bool SequenceEqual<T>( this IEnumerable<T> self, params T[] other ) => self.SequenceEqual( EnumerableOf( other ) );
	public static bool UnorderedEqual<T>( this IEnumerable<T> self, params T[] other ) where T : notnull => self.UnorderedEqual( EnumerableOf( other ) );
	public static MutableList<E> Sorted<E, T>( this IEnumerable<E> self, Sys.Func<E, T> extractor, bool descending = false ) where T : Sys.IComparable<T> => self.Sorted( extractor, DefaultComparator, descending );
	public static MutableList<E> Sorted<E>( this IEnumerable<E> self, Sys.Comparison<E> comparator, bool descending = false ) => self.Sorted( DefaultExtractor, comparator, descending );
	public static MutableList<E> Sorted<E>( this IEnumerable<E> self, bool descending = false ) where E : Sys.IComparable<E> => self.Sorted( DefaultExtractor, DefaultComparator, descending );

	public static bool UnorderedEqual<T>( this IEnumerable<T> self, IEnumerable<T> other ) where T : notnull
	{
		//TODO: benchmark the following two, see which one is faster.
		if( True )
		{
			// From https://stackoverflow.com/a/3670089/773113
			Dictionary<T, int> counts = new();
			foreach( T item in self )
				if( !counts.TryAdd( item, 1 ) )
					counts[item]++;
			foreach( T item in other )
				if( counts.TryGetValue( item, out int value ) )
					counts[item] = --value;
				else
					return false;
			return counts.Values.All( count => count == 0 );
		}
		else
		{
			// From https://stackoverflow.com/a/3670082/773113
			self = self.Collect();
			other = other.Collect();
			return self.Count() == other.Count() && !self.Except( other ).Any();
		}
	}

	public static MutableList<E> Sorted<E, T>( this IEnumerable<E> self, Sys.Func<E, T> extractor, Sys.Comparison<T> comparator, bool descending = false )
	{
		MutableList<E> list = new( self );
		int multiplier = descending ? -1 : 1;
		list.Sort( new ComparisonComparer<E>( comparison ) );
		return list;

		int comparison( E a, E b ) => comparator.Invoke( extractor.Invoke( a ), extractor.Invoke( b ) ) * multiplier;
	}

	public static ImmutableArray<T> Collect<T>( this IEnumerable<T> self ) => self.ToImmutableArray();

#pragma warning disable RS0030 // RS0030: "Do not use banned APIs"
	public static T[] ToArraySeriously<T>( this IEnumerable<T> self ) => self.ToArray();
#pragma warning restore RS0030 // RS0030: "Do not use banned APIs"

	public static void CopyTo<E>( this IReadOnlyCollection<E> self, E[] array, int arrayIndex )
	{
		foreach( E item in self )
			array[arrayIndex++] = item;
	}

	public static void AddRange<T>( this ICollection<T> self, IEnumerable<T> other )
	{
		foreach( T element in other )
			self.Add( element );
	}

	public static bool AddRange<T>( this ISet<T> self, IEnumerable<T> other )
	{
		bool added = false;
		foreach( T element in other )
			if( self.Add( element ) )
				added = true;
		return added;
	}

	public static bool LegacyContains( this LegacyCollections.IList self, object value )
	{
		foreach( object v in self )
			if( Equals( v, value ) )
				return true;
		return false;
	}

	public static IEnumerable<T> Concat<T>( this IEnumerable<T> self, params T[] other )
	{
		// PEARL: ReSharper suggests to convert this call to an extension method call;
		// if you do, it converts it and it thinks that it is invoking the extension method in Linq.Enumerable;
		// however, the C# compiler thinks otherwise: it invokes this same method instead,
		// which of course miserably fails with a stack overflow.
		// the only solution I have been able to come up with is to disable the inspection.
		// ReSharper disable once InvokeAsExtensionMethod
		return Enumerable.Concat( self, other );
	}

	public static IEnumerable<T> LegacyAsEnumerable<T>( this LegacyCollections.IEnumerable self )
	{
		foreach( T element in self )
			yield return element;
	}

	public static T ExtractAt<T>( this IList<T> self, int index )
	{
		T result = self[index];
		self.RemoveAt( index );
		return result;
	}

	//public static V Extract<K, V>( this IDictionary<K, V> self, K key ) => KitHelpers.Extract( self, key );
	//public static V? TryExtract<K, V>( this IDictionary<K, V> self, K key ) where K : notnull where V : class => KitHelpers.TryExtract( self, key );

	//PEARL: the "add-item-to-set" method of DotNet is not really an "add" method, it is actually a "try-add" method,
	//       because instead of failing if the item already exists, it returns a boolean to indicate what happened.
	//       So, if we want a real "add" function which does not fail to fail, (duh!) we have to write it ourselves.
	//       Unfortunately, the name `Add` is taken, so  we have to give the new function a different name.
	public static void DoAdd<T>( this ISet<T> self, T element )
	{
		bool ok = self.Add( element );
		Assert( ok );
	}

	//PEARL: the "remove-item-from-collection" method of DotNet is not really a "remove" method, it is actually a
	//       "try-remove" method, because instead of failing if the item does not exist, it returns a boolean to
	//       indicate what happened.
	//       So, if we want a real "remove" function which does not fail to fail, (duh!) we have to write it ourselves.
	//       Unfortunately, the name `Remove` is taken, so we have to give the new function a different name.
	public static void DoRemove<T>( this ICollection<T> self, T element )
	{
		bool ok = self.Remove( element );
		Assert( ok );
	}

	//PEARL: the "remove-item-from-dictionary" method of DotNet is not really a "remove" method, it is actually a
	//       "try-remove" method, because instead of failing if the key is not found, it returns a boolean to indicate
	//       what happened.
	//       So, if we want a real "remove" function which does not fail to fail, (duh!) we have to write it ourselves.
	//       Unfortunately, the name `Remove` is taken, so we have to give the new function a different name.
	public static void DoRemove<K, V>( this IDictionary<K, V> self, K key )
	{
		bool ok = self.Remove( key );
		Assert( ok );
	}

	public static void Move<T>( this IList<T> self, int oldIndex, int newIndex )
	{
		if( oldIndex == newIndex )
			return;
		T item = self[oldIndex];
		self.RemoveAt( oldIndex );
		self.Insert( newIndex, item );
	}

	//public static V? TryGet<K, V>( this IDictionary<K, V> self, K key ) where K : notnull where V : notnull => KitHelpers.TryGet( self, key );
	public static bool ContainsAll<T>( this ICollection<T> self, IEnumerable<T> items ) => items.All( self.Contains );
	public static bool ContainsAny<T>( this ICollection<T> self, IEnumerable<T> items ) => items.Any( self.Contains );
	public static bool IsEmpty<T>( this IReadOnlyCollection<T> self ) => self.Count == 0;
	public static bool IsEmpty<T>( this IEnumerable<T> self ) => !self.Any();

	public static IEnumerable<R> SelectWhereNonNull<T, R>( this IEnumerable<T> source, Sys.Func<T, R?> selector ) where R : notnull
	{
		foreach( T item in source )
		{
			R? convertedItem = selector.Invoke( item );
			if( convertedItem is null )
				continue;
			yield return convertedItem;
		}
	}

	public static IEnumerable<T> WhereNonNull<T>( this IEnumerable<T?> source ) where T : struct
	{
		foreach( T? item in source )
		{
			if( item is null )
				continue;
			yield return item.Value;
		}
	}

	// See Stack Overflow: How to combine `Select` and `Where` https://stackoverflow.com/q/75844910/773113
	public static IEnumerable<R> SelectWhere<T, R>( this IEnumerable<T> source, Sys.Func<T, (bool, R?)> selector )
	{
		foreach( T item in source )
		{
			(bool include, R? value) = selector( item );
			if( include )
				yield return value!;
		}
	}

	// See Stack Overflow: How to combine `Select` and `Where` https://stackoverflow.com/q/75844910/773113
	public static IEnumerable<R> SelectWhereNotNull<T, R>( this IEnumerable<T> source, Sys.Func<T, R?> selector ) where T : notnull
	{
		foreach( T item in source )
		{
			R? value = selector( item );
			if( value != null )
				yield return value;
		}
	}

	public static bool UpdateFrom<K, V>( this IDictionary<K, V> self, IReadOnlyDictionary<K, V> other ) where K : notnull
	{
		bool changed = false;
		HashSet<V> found = new();
		foreach( K key in other.Keys )
		{
			if( !self.TryGetValue( key, out V? element ) )
				element = other[key];
			found.Add( element );
			changed = true;
		}
		foreach( (K key, V element) in self.Collect() )
			if( !found.Contains( element ) )
			{
				self.Remove( key );
				changed = true;
			}
		return changed;
	}

	public static MutableList<T> Reversed<T>( this IEnumerable<T> self )
	{
		MutableList<T> mutableList = new( self );
		mutableList.Reverse();
		return mutableList;
	}

	public static IEnumerable<R> Select2<K, V, R>( this IReadOnlyDictionary<K, V> source, Sys.Func<K, V, R> selector ) where K : notnull
	{
		return source.Select( keyValuePair => selector.Invoke( keyValuePair.Key, keyValuePair.Value ) );
	}

	/// <summary>Checks whether all items have the same value.</summary>
	/// <remarks>Fails if there are no items.</remarks>
	/// <param name="self">The enumerable.</param>
	/// <returns><c>true</c> if all items in have the same value, <c>false</c> otherwise.</returns>
	public static bool AreAllSame<T>( this IEnumerable<T> self ) where T : notnull
	{
		using( IEnumerator<T> enumerator = self.GetEnumerator() )
		{
			bool hasFirst = enumerator.MoveNext();
			Assert( hasFirst );
			T firstItem = enumerator.Current;
			while( enumerator.MoveNext() )
				if( !DotNetHelpers.Equals( firstItem, enumerator.Current ) )
					return false;
		}
		return true;
	}
}
