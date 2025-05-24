namespace MikeNakis.Kit.Extensions;

using MikeNakis.Kit.Collections;

///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
public static class MiscellaneousExtensions
{
	public static string ToString2( this byte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this sbyte self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this char self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this short self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this int self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this uint self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this uint self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this long self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.Int128 self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this float self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this double self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this decimal self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.NumericFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self ) => self.ToString( SysGlob.CultureInfo.InvariantCulture );
	public static string ToString2( this Sys.DateTime self, [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.DateTimeFormat )] string? format ) => self.ToString( format, SysGlob.CultureInfo.InvariantCulture );
	public static void OrThrow( this bool self ) => _ = self ? true : throw new AssertionFailureException();
	public static void OrThrow( this bool self, Sys.Func<Sys.Exception> exceptionFactory ) => _ = self ? true : throw exceptionFactory.Invoke();

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

	public static bool ReferenceEquals<T>( this T? self, T? other ) where T : class
	{
#pragma warning disable RS0030 // Do not use banned APIs
		return object.ReferenceEquals( self, other );
#pragma warning restore RS0030 // Do not use banned APIs
	}

	public static double Clamped( this double self, double min, double max )
	{
		Assert( min <= max );
		if( self < min )
			return min;
		if( self > max )
			return max;
		return self;
	}

	public static float Clamped( this float self, float min, float max )
	{
		Assert( min < max );
		if( self < min )
			return min;
		if( self > max )
			return max;
		return self;
	}

	public static string GetCSharpName( this Sys.Type type, bool useAliases = true )
	{
		return CSharpTypeNameGenerator.GetCSharpTypeName( type, useAliases );
	}
}
