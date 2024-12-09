namespace MikeNakis.Kit;

using CodeAnalysis = System.Diagnostics.CodeAnalysis;
using Sys = System;
using SysGlob = System.Globalization;
using SysText = System.Text;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
///<remarks>NOTE: Each method must do nothing but delegate to some other class which does the job via a non-extension method.</remarks>
public static class GlobalExtensions
{
	public static string Replace2( this string self, string oldValue, string? newValue ) => self.Replace( oldValue, newValue, Sys.StringComparison.Ordinal );
	public static bool Equals2( this string self, [CodeAnalysis.NotNullWhen( true )] string? value ) => self.Equals( value, Sys.StringComparison.Ordinal );
	public static bool EqualsIgnoreCase( this string self, [CodeAnalysis.NotNullWhen( true )] string? value ) => self.Equals( value, Sys.StringComparison.OrdinalIgnoreCase );
	public static bool StartsWith2( this string self, string s ) => self.StartsWith( s, Sys.StringComparison.Ordinal );
	public static bool StartsWithIgnoreCase( this string self, string s ) => self.StartsWith( s, Sys.StringComparison.OrdinalIgnoreCase );
	public static int IndexOf2( this string self, char c ) => self.IndexOf( c, Sys.StringComparison.Ordinal );
	public static int IndexOf2( this string self, string s ) => self.IndexOf( s, Sys.StringComparison.Ordinal );
	public static int IndexOf2( this string self, string s, int startIndex, int count ) => self.IndexOf( s, startIndex, count, Sys.StringComparison.Ordinal );
	public static bool EndsWith2( this string self, string s ) => self.EndsWith( s, Sys.StringComparison.Ordinal );
	public static bool EndsWithIgnoreCase( this string self, string s ) => self.EndsWith( s, Sys.StringComparison.OrdinalIgnoreCase );
	public static bool Contains2( this string self, char c ) => self.Contains( c, Sys.StringComparison.Ordinal );
	public static int GetHashCode2( this string self ) => self.GetHashCode( Sys.StringComparison.Ordinal );
	public static string ToUpper2( this string self ) => self.ToUpper( SysGlob.CultureInfo.InvariantCulture );
	public static SysText.StringBuilder Append2( this SysText.StringBuilder self, string s ) => self.Append( s );

	public static string ToString2( this byte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this sbyte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this char self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this short self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this long self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.Int128 self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this double self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this decimal self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self, [CodeAnalysis.StringSyntax( CodeAnalysis.StringSyntaxAttribute.DateTimeFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );

	public static bool Equals2( this Sys.ReadOnlySpan<char> span, Sys.ReadOnlySpan<char> other ) => Sys.MemoryExtensions.Equals( span, other, Sys.StringComparison.Ordinal );

	public static StringSlicer Slice( this string source, char separator ) => new( source, 0, source.Length, separator );
	public static StringSlicer Slice( this string source, int start, int end, char separator ) => new( source, start, end, separator );
}
