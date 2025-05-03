namespace MikeNakis.Kit.Extensions;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class StringExtensions
{
	public static string Replace2( this string self, string oldValue, string? newValue ) => self.Replace( oldValue, newValue, Sys.StringComparison.Ordinal );
	public static bool Equals2( this string self, [SysCodeAnalysis.NotNullWhen( true )] string? value ) => self.Equals( value, Sys.StringComparison.Ordinal );
	public static bool EqualsIgnoreCase( this string self, [SysCodeAnalysis.NotNullWhen( true )] string? value ) => self.Equals( value, Sys.StringComparison.OrdinalIgnoreCase );
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
	public static StringSlicer Slice( this string source, char separator ) => new( source, 0, source.Length, separator );
	public static StringSlicer Slice( this string source, int start, int end, char separator ) => new( source, start, end, separator );
}
