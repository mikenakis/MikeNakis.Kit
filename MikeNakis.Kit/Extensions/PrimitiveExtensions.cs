namespace MikeNakis.Kit.Extensions;

using CodeAnalysis = System.Diagnostics.CodeAnalysis;
using Sys = System;
using SysGlob = System.Globalization;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class PrimitiveExtensions
{
	public static string ToString2( this byte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this sbyte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this char self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this short self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this uint self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this uint self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this long self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.Int128 self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this double self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this decimal self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.DateTimeFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static void OrThrow( this bool self ) => _ = self ? true : throw new AssertionFailureException();
	public static void OrThrow( this bool self, Sys.Func<Sys.Exception> exceptionFactory ) => _ = self ? true : throw exceptionFactory.Invoke();

	public static string SafeSubstring( this string self, int startIndex ) => self.SafeSubstring( startIndex, startIndex >= self.Length ? 0 : self.Length - startIndex );
	public static string SafeSubstring( this string self, int startIndex, int length )
	{
		Assert( startIndex >= 0 );
		Assert( length >= 0 );
		if( startIndex > self.Length )
			startIndex = self.Length;
		if( length - startIndex > self.Length )
			length = self.Length - startIndex;
		return self.Substring( startIndex, length );
	}
}
