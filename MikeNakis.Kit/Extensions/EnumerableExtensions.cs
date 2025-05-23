namespace MikeNakis.Kit.Extensions;

using MikeNakis.Kit;
using MikeNakis.Kit.Collections;

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
}
