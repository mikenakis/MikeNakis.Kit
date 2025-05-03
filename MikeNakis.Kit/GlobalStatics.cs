namespace MikeNakis.Kit;

using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;

///<summary>Frequently used stuff that needs to be conveniently accessible without a type name qualification.</summary>
///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
///<remarks>NOTE: This can be used either with <code>global using static</code> in _GlobalUsings.cs or with <code>using static</code> in each source file.</remarks>
public static class GlobalStatics
{
	///<summary>Always returns <c>true</c>.</summary>
	///<remarks>Same as <c>if( true )</c>, but without a "condition is always true" warning.
	///Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool True => true;

	///<summary>Always returns <c>false</c>.</summary>
	///<remarks>Same as <c>if( false )</c>, but without a "condition is always false" warning.
	///Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool False => false;

	///<summary>Returns <c>true</c> if <c>DEBUG</c> has been defined.</summary>
	///<remarks>Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
#if DEBUG
	public static bool DebugMode => true;
#else
	public static bool DebugMode => false;
#endif

	public static bool Debugging => DebugMode && SysDiag.Debugger.IsAttached;

	///<summary>Identity function.</summary>
	///<remarks>useful as a no-op lambda and sometimes as a debugging aid.</remarks>
	public static T Identity<T>( T value ) => value;

	///<summary>Compares two <c>double</c> values for approximate equality.</summary>
	//TODO: perhaps replace with something more sophisticated, like this: https://stackoverflow.com/a/3875619/773113
	public static bool DoubleEquals( double a, double b, double? tolerance = null )
	{
		if( double.IsNaN( a ) && double.IsNaN( b ) )
			return true;
		double difference = Math.Abs( a - b );
		return difference < (tolerance ?? KitHelpers.Epsilon);
	}

	///<summary>Compares two <c>double</c> values for exact equality.</summary>
	///<remarks>Avoids the "equality comparison of floating point numbers" inspection of ReSharper, which is (badly)
	///missing from dotnet analyzers.</remarks>
	public static bool DoubleExactlyEquals( double a, double b )
	{
		return a.Equals( b );
	}

	///<summary>Compares two <c>float</c> values for approximate equality.</summary>
	//TODO: perhaps replace with something more sophisticated, like this: https://stackoverflow.com/a/3875619/773113
	public static bool FloatEquals( float a, float b, float? tolerance = null )
	{
		if( float.IsNaN( a ) && float.IsNaN( b ) )
			return true;
		float difference = Math.Abs( a - b );
		return difference < (tolerance ?? KitHelpers.FEpsilon);
	}

	///<summary>Compares two <c>float</c> values for exact equality.</summary>
	///<remarks>Avoids the "equality comparison of floating point numbers" inspection of ReSharper, which is (badly)
	///missing from dotnet analyzers.</remarks>
	public static bool FloatExactlyEquals( float a, float b )
	{
		return a.Equals( b );
	}

	public static bool StringEquals( string? a, string? b ) => string.Equals( a, b, Sys.StringComparison.Ordinal );
	public static int StringCompare( string? a, string? b ) => string.Compare( a, b, Sys.StringComparison.Ordinal );
	public static int StringCompareIgnoreCase( string? a, string? b ) => string.Compare( a, b, Sys.StringComparison.OrdinalIgnoreCase );
	public static char CharToUpper( char c ) => char.ToUpper( c, SysGlob.CultureInfo.InvariantCulture );
	public static string StringFormat( [SysCodeAnalysis.StringSyntax( SysCodeAnalysis.StringSyntaxAttribute.CompositeFormat )] string format, params object?[] arguments ) => string.Format( SysGlob.CultureInfo.InvariantCulture, format, arguments );

	public static string Id<T>( T o ) where T : class => $"{DotNetHelpers.GetFriendlyTypeName( o.GetType(), false )}@{DotNetHelpers.IdentityHashCode( o ).ToString2( "x8" )}";

	/// <summary>Performs an assertion.</summary>
	/// <remarks>Invokes the supplied <paramref name="check" /> function, passing it the supplied <paramref name="value"/>.
	/// If the <paramref name="check"/> function returns <c>false</c>,
	/// then the <paramref name="value"/> is passed to the supplied <paramref name="exceptionFactory"/> function, and the returned <see cref="Sys.Exception"/> is thrown.
	/// (Though the factory may just as well throw the exception instead of returning it.)
	/// This function is only executed (and the supplied <paramref name="value"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert<T>( T value, Sys.Func<T, bool> check, Sys.Func<T, Sys.Exception> exceptionFactory )
	{
		if( check.Invoke( value ) )
			return;
		fail( () => exceptionFactory.Invoke( value ) );
	}

	/// <summary>Performs an assertion.</summary>
	/// <remarks>If the given <paramref name="condition"/> is <c>false</c>, the supplied <paramref name="exceptionFactory"/> is invoked, and the returned <see cref="Sys.Exception"/> is thrown.
	/// (Though the factory may just as well throw the exception instead of returning it.)
	/// This function is only executed (and the supplied <paramref name="condition"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert( [SysCodeAnalysis.DoesNotReturnIf( false )] bool condition, Sys.Func<Sys.Exception> exceptionFactory ) //
	{
		if( condition )
			return;
		fail( exceptionFactory );
	}

	/// <summary>Performs an assertion.</summary>
	/// <remarks>If the given <paramref name="condition"/> is <c>false</c>, an <see cref="AssertionFailureException"/> is thrown.
	/// This function is only executed (and the supplied <paramref name="condition"/> is only evaluated) when running a debug build.</remarks>
	[SysDiag.DebuggerHidden, SysDiag.Conditional( "DEBUG" )]
	public static void Assert( [SysCodeAnalysis.DoesNotReturnIf( false )] bool condition ) //
	{
		if( condition )
			return;
		fail( () => new AssertionFailureException() );
	}

	[SysDiag.DebuggerHidden]
	static void fail( Sys.Func<Sys.Exception> exceptionFactory )
	{
		Sys.Exception exception = exceptionFactory.Invoke();
		if( !KitHelpers.FailureTesting.Value )
		{
			SysDiag.Debug.WriteLine( $"Assertion failed: {exception.GetType().FullName}: {exception.Message}" );
			if( Breakpoint() )
				return;
		}
		throw exception;
	}

	/// <summary>Returns the supplied pointer unchanged, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableReference ) where T : class //
	{
		Assert( nullableReference != null );
		return nullableReference;
	}

	/// <summary>Returns the supplied pointer unchanged, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableReference, Sys.Func<Sys.Exception> exceptionFactory ) where T : class //
	{
		Assert( nullableReference != null, exceptionFactory );
		return nullableReference;
	}

	/// <summary>Converts a nullable value to non-nullable, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableValue ) where T : struct //
	{
		Assert( nullableValue.HasValue );
		return nullableValue.Value;
	}

	/// <summary>Converts a nullable value to non-nullable, while asserting that it is non-<c>null</c>.</summary>
	[SysDiag.DebuggerHidden]
	public static T NotNull<T>( T? nullableValue, Sys.Func<Sys.Exception> exceptionFactory ) where T : struct //
	{
		Assert( nullableValue.HasValue, exceptionFactory );
		return nullableValue.Value;
	}

	[SysDiag.DebuggerHidden]
#pragma warning disable CA1021 //CA1021: "Avoid `out` parameters"
	public static void NotNullCast<T, U>( T? input, out U output ) where U : T where T : class => output = (U)NotNull( input );
#pragma warning restore CA1021 //CA1021: "Avoid `out` parameters"

	/// <summary>If a debugger is attached, hits a breakpoint and returns <c>true</c>; otherwise, returns <c>false</c></summary>
	[SysDiag.DebuggerHidden]
	public static bool Breakpoint()
	{
		if( SysDiag.Debugger.IsAttached )
		{
			SysDiag.Debugger.Break(); //Note: this is problematic due to some Visual Studio bug: when it hits, you are prevented from setting the next statement either within the calling function or within this function.
			return true;
		}
		return false;
	}

	[SysDiag.DebuggerHidden] public static Sys.Exception Failure() => throw new AssertionFailureException();
	[SysDiag.DebuggerHidden] public static Sys.Exception Failure( string message ) => throw new AssertionFailureException( message );
	[SysDiag.DebuggerHidden] public static Sys.Exception Failure( string message, Sys.Exception cause ) => throw new AssertionFailureException( message, cause );

	public static IEnumerable<T> EnumerableOf<T>() => ReadOnlyListOf<T>();
	public static IEnumerable<T> EnumerableOf<T>( T element ) => ReadOnlyListOf( element );
	public static IEnumerable<T> EnumerableOf<T>( T element1, T element2 ) => ReadOnlyListOf( element1, element2 );
	public static IEnumerable<T> EnumerableOf<T>( T element1, T element2, T element3 ) => ReadOnlyListOf( element1, element2, element3 );
	public static IEnumerable<T> EnumerableOf<T>( params T[] elements ) => ReadOnlyListOf( elements );
	public static IEnumerable<T> EnumerableOfOrEmpty<T>( T? element ) => element == null ? Enumerable.Empty<T>() : ReadOnlyListOf( element );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>() => ReadOnlyListOf<T>();
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element ) => ReadOnlyListOf( element );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element1, T element2 ) => ReadOnlyListOf( element1, element2 );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element1, T element2, T element3 ) => ReadOnlyListOf( element1, element2, element3 );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( params T[] elements ) => ReadOnlyListOf( elements );
	public static IReadOnlyList<T> ReadOnlyListOf<T>() => ArrayWrapper.Of<T>();
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element ) => ArrayWrapper.Of( element );
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element1, T element2 ) => ArrayWrapper.Of( element1, element2 );
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element1, T element2, T element3 ) => ArrayWrapper.Of( element1, element2, element3 );
	public static IReadOnlyList<T> ReadOnlyListOf<T>( params T[] elements ) => ArrayWrapper.Of( elements );

	public static Sys.Exception? TryCatch( Sys.Action procedure )
	{
		Assert( !KitHelpers.FailureTesting.Value );
		KitHelpers.FailureTesting.Value = true;
		try
		{
			procedure.Invoke();
			return null;
		}
		catch( Sys.Exception exception )
		{
			return exception;
		}
		finally
		{
			KitHelpers.FailureTesting.Value = false;
		}
	}

	public static string S( Sys.FormattableString formattable )
	{
		return formattable.ToString( SysGlob.CultureInfo.InvariantCulture );
	}
}
