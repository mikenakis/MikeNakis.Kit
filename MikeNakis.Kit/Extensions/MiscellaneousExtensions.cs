namespace MikeNakis.Kit.Extensions;

using MikeNakis.Kit.Collections;
using Sys = System;
using SysText = System.Text;
using SysDiag = System.Diagnostics;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class MiscellaneousExtensions
{
	public static SysText.StringBuilder Append2( this SysText.StringBuilder self, string s ) => self.Append( s );

	public static bool Equals2( this Sys.ReadOnlySpan<char> span, Sys.ReadOnlySpan<char> other ) => span.Equals( other, Sys.StringComparison.Ordinal );

	public static IReadOnlyCollection<T> AsReadOnly<T>( this ICollection<T> self ) => new ReadOnlyCollectionOnCollection<T>( self );

	[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
	sealed class ReadOnlyCollectionOnCollection<T> : AbstractReadOnlyCollection<T>
	{
		readonly ICollection<T> collection;
		public ReadOnlyCollectionOnCollection( ICollection<T> collection ) => this.collection = collection;
		public override int Count => collection.Count;
		public override IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
		public override string? ToString() => collection.ToString();
	}
}
