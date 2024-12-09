namespace MikeNakis.Kit.Extensions;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit.IO;
using Sys = System;
using SysText = System.Text;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
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
}
