namespace MikeNakis.Kit.Collections;

using MikeNakis.Kit;
using MikeNakis.Kit.Extensions;

// TODO: get rid of this class, move its contents to Such-And-Such-Extensions.cs
public static class LinqEx
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

	public static X DefaultExtractor<X>( X a ) => a;
	public static int DefaultComparator<T>( T a, T b ) where T : Sys.IComparable<T> => a.CompareTo( b );

	public static IReadOnlyList<U> Select<T, U>( this IReadOnlyList<T> self, Sys.Func<T, U> converter ) => new SelectList<T, U>( self, ( t, _ ) => converter.Invoke( t ) );
	public static IReadOnlyList<U> Select<T, U>( this IReadOnlyList<T> self, Sys.Func<T, int, U> converter ) => new SelectList<T, U>( self, converter );
	public static IReadOnlyList<U> SelectMany<T, U>( this IReadOnlyList<T> self, Sys.Func<T, IReadOnlyList<U>> converter ) => new SelectManyList<T, U>( self, converter );
	public static bool SequenceEqual<T>( this IEnumerable<T> self, params T[] other ) => self.SequenceEqual( EnumerableOf( other ) );
	public static bool UnorderedEqual<T>( this IEnumerable<T> self, params T[] other ) where T : notnull => UnorderedEqual( self, EnumerableOf( other ) );
	public static void Sort<T, E>( this MutableList<T> self, Sys.Func<T, E> extractor, bool descending = false ) where E : Sys.IComparable<E> => self.Sort( extractor, DefaultComparator, descending );
	public static void Sort<T>( this MutableList<T> self, bool descending = false ) where T : Sys.IComparable<T> => self.Sort( DefaultExtractor, DefaultComparator, descending );
	public static MutableList<E> Sorted<E, T>( this IEnumerable<E> self, Sys.Func<E, T> extractor, bool descending = false ) where T : Sys.IComparable<T> => self.Sorted( extractor, DefaultComparator, descending );
	public static MutableList<E> Sorted<E>( this IEnumerable<E> self, Sys.Comparison<E> comparator, bool descending = false ) => self.Sorted( DefaultExtractor, comparator, descending );
	public static MutableList<E> Sorted<E>( this IEnumerable<E> self, bool descending = false ) where E : Sys.IComparable<E> => self.Sorted( DefaultExtractor, DefaultComparator, descending );
	public static int BinarySearch<E, T>( this IReadOnlyList<E> self, T item, Sys.Func<E, T> extractor, Sys.Comparison<T> comparison, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, extractor, comparison, skipDuplicates );
	public static int BinarySearch<E>( this IReadOnlyList<E> self, E item, Sys.Comparison<E> comparison, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, comparison, skipDuplicates );
	public static int BinarySearch<T>( this IReadOnlyList<T> self, T item, IComparer<T> comparer, bool skipDuplicates = false ) => KitHelpers.BinarySearch( self, item, comparer, skipDuplicates );
	public static int BinarySearch<E, T>( this IReadOnlyList<E> self, T item, Sys.Func<E, T> extractor, bool skipDuplicates = false ) where T : Sys.IComparable<T> => KitHelpers.BinarySearch( self, item, extractor, Comparer<T>.Default, skipDuplicates );
	public static int BinarySearch<E>( this IReadOnlyList<E> self, E item, bool skipDuplicates = false ) where E : Sys.IComparable<E> => KitHelpers.BinarySearch( self, item, Comparer<E>.Default, skipDuplicates );

	public static void CopyTo<E>( this IReadOnlyCollection<E> self, E[] array, int arrayIndex )
	{
		foreach( E item in self )
			array[arrayIndex++] = item;
	}

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

	public static IReadOnlyList<T> Collect<T>( this IReadOnlyList<T> self ) => new ArrayWrapper<T>( self );

#pragma warning disable RS0030 // RS0030: "Do not use banned APIs"
	public static IReadOnlyList<T> Collect<T>( this IEnumerable<T> self ) => ReadOnlyListOf( self.ToArray() );
	public static T[] ToArraySeriously<T>( this IEnumerable<T> self ) => self.ToArray();
#pragma warning restore RS0030 // RS0030: "Do not use banned APIs"

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

	public static T? LastOrDefault<T>( this IReadOnlyList<T> self ) => self.Count == 0 ? default : self[^1];
}
