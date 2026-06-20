namespace MikeNakis.Kit.Extensions;

using MikeNakis.Kit.Collections;
using Sys = System;

public static class ListExtensions
{
	// We cannot declare this because it conflicts with IEnumerable.IsEmpty().
	// TODO: remove IEnumerable.IsEmpty(), because we already have .Any(), and enable this extension method.
	// public static bool IsEmpty<T>( this IList<T> self ) => self.Count == 0;
	public static void Sort<T, E>( this MutableList<T> self, Sys.Func<T, E> extractor, bool descending = false ) where E : Sys.IComparable<E> => self.Sort( extractor, EnumerableExtensions.DefaultComparator, descending );
	public static void Sort<T>( this MutableList<T> self, bool descending = false ) where T : Sys.IComparable<T> => self.Sort( EnumerableExtensions.DefaultExtractor, EnumerableExtensions.DefaultComparator, descending );
}
