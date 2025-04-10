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
	public static bool OrThrow( this bool self ) => self ? throw new AssertionFailureException() : true;
	public static bool OrThrow( this bool self, Sys.Func<Sys.Exception> exceptionFactory ) => self ? throw exceptionFactory.Invoke() : true;
}
