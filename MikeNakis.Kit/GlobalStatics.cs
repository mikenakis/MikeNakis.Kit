namespace MikeNakis.Kit;

using MikeNakis.Kit.Collections;
using Sys = System;
using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;
using SysGlob = System.Globalization;

///<summary>Frequently used stuff that needs to be conveniently accessible without a type name qualification.</summary>
///<remarks>NOTE: This class must be kept AS SMALL AS POSSIBLE.</remarks>
///<remarks>NOTE: This can be used either with <code>global using static</code> in _GlobalUsings.cs or with <code>using static</code> in each source file.</remarks>
///<remarks>NOTE: each method must do nothing but delegate to some method that does the job in a class which is not statically imported.</remarks>
public static class GlobalStatics
{
	///<summary>Always returns <c>true</c>.</summary>
	///<remarks>Same as <c>if( true )</c>, but without a "condition is always true" warning.</remarks>
	///<remarks>Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool True => true;

	///<summary>Always returns <c>false</c>.</summary>
	///<remarks>Same as <c>if( false )</c>, but without a "condition is always false" warning.</remarks>
	///<remarks>Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
	public static bool False => false;

	///<summary>Returns <c>true</c> if <c>DEBUG</c> has been defined.</summary>
	///<remarks>Allows code to be enabled/disabled while still having to pass compilation, thus preventing code rot.</remarks>
#pragma warning disable IDE0025 //IDE0025: "Use expression body for properties"
	public static bool DebugMode
	{
		get
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}
#pragma warning restore IDE0025 //IDE0025: "Use expression body for properties"

	public static bool Debugging => DebugMode && SysDiag.Debugger.IsAttached;

	///<summary>Identity function.</summary>
	///<remarks>useful as a no-op lambda and sometimes as a debugging aid.</remarks>
	public static T Identity<T>( T value ) => value;

	public static bool DoubleEquals( double a, double b, double? tolerance = null ) => KitHelpers.DoubleEquals( a, b, tolerance );
	public static bool DoubleExactlyEquals( double a, double b ) => KitHelpers.DoubleExactlyEquals( a, b );
	public static bool FloatEquals( float a, float b, float? tolerance = null ) => KitHelpers.FloatEquals( a, b, tolerance );
	public static bool FloatExactlyEquals( float a, float b ) => KitHelpers.FloatExactlyEquals( a, b );

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

	public static T WithAssertion<T>( this T self, Sys.Func<bool> assertion )
	{
		Assert( assertion.Invoke() );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<T, bool> assertion )
	{
		Assert( assertion.Invoke( self ) );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<bool> assertion, Sys.Func<Sys.Exception> exceptionFactory )
	{
		Assert( self, _ => assertion.Invoke(), _ => exceptionFactory.Invoke() );
		return self;
	}

	public static T WithAssertion<T>( this T self, Sys.Func<T, bool> assertion, Sys.Func<T, Sys.Exception> exceptionFactory )
	{
		Assert( self, assertion, exceptionFactory.Invoke );
		return self;
	}

	public static T WithNotNullAssertion<T>( this T? self )
	{
		Assert( self is not null );
		return self!;
	}

	public static U As<U>( this Sys.Exception self ) where U : Sys.Exception
	{
		return (U)self;
	}

	[SysDiag.DebuggerHidden]
	static void fail( Sys.Func<Sys.Exception> exceptionFactory )
	{
		Sys.Exception exception = exceptionFactory.Invoke();
		if( Debugging && !KitHelpers.FailureTesting.Value )
		{
			Log.Error( "Assertion failed", exception );
			Breakpoint();
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
	public static void Breakpoint()
	{
		if( SysDiag.Debugger.IsAttached )
			SysDiag.Debugger.Break(); //Note: this is problematic due to some Visual Studio bug: when it hits, you are prevented from setting the next statement either within the calling function or within this function.
	}

#pragma warning disable CA2201 // Do not raise reserved exception types
	public static T Failure<T>( string? message = null ) => throw new Sys.Exception( message );
#pragma warning restore CA2201 // Do not raise reserved exception types

	public static IEnumerable<T> EnumerableOf<T>() => ReadOnlyListOf<T>();
	public static IEnumerable<T> EnumerableOf<T>( T element ) => ReadOnlyListOf( element );
	public static IEnumerable<T> EnumerableOf<T>( T element1, T element2 ) => ReadOnlyListOf( element1, element2 );
	public static IEnumerable<T> EnumerableOf<T>( T element1, T element2, T element3 ) => ReadOnlyListOf( element1, element2, element3 );
	public static IEnumerable<T> EnumerableOf<T>( params T[] elements ) => ReadOnlyListOf( elements );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>() => ReadOnlyListOf<T>();
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element ) => ReadOnlyListOf( element );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element1, T element2 ) => ReadOnlyListOf( element1, element2 );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( T element1, T element2, T element3 ) => ReadOnlyListOf( element1, element2, element3 );
	public static IReadOnlyCollection<T> ReadOnlyCollectionOf<T>( params T[] elements ) => ReadOnlyListOf( elements );
	public static IReadOnlyList<T> ReadOnlyListOf<T>() => Series.Of<T>().AsReadOnlyList();
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element ) => Series.Of( element ).AsReadOnlyList();
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element1, T element2 ) => Series.Of( element1, element2 ).AsReadOnlyList();
	public static IReadOnlyList<T> ReadOnlyListOf<T>( T element1, T element2, T element3 ) => Series.Of( element1, element2, element3 ).AsReadOnlyList();
	public static IReadOnlyList<T> ReadOnlyListOf<T>( params T[] elements ) => Series.Of( elements ).AsReadOnlyList();

	public static IEnumerable<T> Except<T>( this IEnumerable<T> enumerable, T item ) => enumerable.Where( e => !Equals( e, item ) );
	public static IEnumerable<T> Except<T>( this IEnumerable<T> enumerable, params T[] items ) => enumerable.Except( EnumerableOf( items ) );

	public static object[] LegacyEnumerableToArray( this Sys.Collections.IEnumerable enumerable )
	{
		int capacity = 4;
		int size = 0;
		object[] array = new object[capacity];
		foreach( object item in enumerable )
		{
			if( size >= capacity )
			{
				capacity *= 2;
				object[] array2 = new object[capacity];
				Sys.Array.Copy( array, array2, size );
				array = array2;
			}
			array[size++] = item;
		}
		if( size != capacity )
		{
			object[] array2 = new object[size];
			Sys.Array.Copy( array, array2, size );
			array = array2;
		}
		return array;
	}

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
}
