namespace MikeNakis.Kit.Extensions;

using Sys = System;
using SysText = System.Text;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class GlobalExtensions
{
	public static SysText.StringBuilder Append2( this SysText.StringBuilder self, string s ) => self.Append( s );

	public static bool Equals2( this Sys.ReadOnlySpan<char> span, Sys.ReadOnlySpan<char> other ) => span.Equals( other, Sys.StringComparison.Ordinal );

	public static V ComputeIfAbsent<K, V>( this IDictionary<K, V> self, K key, Sys.Func<K, V> factory ) => KitHelpers.ComputeIfAbsent( self, key, factory );
}
