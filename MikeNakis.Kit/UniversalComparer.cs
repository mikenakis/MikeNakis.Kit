namespace MikeNakis.Kit;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpTypeNames.Extensions;
using MikeNakis.Kit.Extensions;
using static GlobalStatics;
using Legacy = System.Collections;
using Math = System.Math;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysReflect = System.Reflection;

// PEARL: Arrays in C# implement `IEnumerable` but provide no implementation for `Equals()`, defaulting to
//		`DotNetHelpers.ReferenceEquals()`!
//		The same holds true for `List<>`, `Dictionary<>`, etc.
//      This means that `object.Equals( array1, array2 )`, `object.Equals( list1, list2 )` and
//      `object.Equals( dictionary1, dictionary2 )` will always return false, even if the collections have
//      identical contents!
//      The standing advice is to use `a.SequenceEqual( b )` to compare instances of `IEnumerable`, but this
//      advise is retarded, due to the following reasons:
//			1. We can only use `SequenceEqual()` if we know in advance the types of the objects being compared; this
//             might be fine for application programmers who are perfectly accustomed to writing copious amounts of
//             mindless application-specific code to accomplish standard tasks, but it does not work when you are
//             writing infrastructure-level code, which operates on data without needing to know, nor wanting to
//             know, the exact type of the data.
//          2. When the elements of an `IEnumerable` are in turn `IEnumerable`, guess what `SequenceEqual()` uses to
//             compare pairs of elements? It uses `object.Equals()`!
//             So, it will miserably fail when the elements are arrays, lists, dictionaries, etc.
//             Again, this might not be a problem for application programmers who will happily write thousands of
//             lines of application-specific code to compare application data having intimate knowledge of the
//             structure of the data, but it does not work when writing infrastructure-level code.
//      This class fixes this insanity. It is meant to be used as a replacement for `object.Equals()`.
sealed class UniversalComparer
{
	public static readonly UniversalComparer Instance = new();

	readonly HashSet<Sys.Type> reportedTypes = new();
	readonly Dictionary<Sys.Type, (SysReflect.PropertyInfo, SysReflect.PropertyInfo)> keyValuePairPropertyInfos = new();

	public new bool Equals( object? a, object? b )
	{
		if( a.ReferenceEquals( b ) )
			return true;
		if( a == null || b == null )
			return false;
		Sys.Type type = a.GetType();
		if( type.IsPrimitive )
			goto end;
		// PEARL: If the values are tuples, we cannot allow ITuple.Equals() to be invoked, because for each pair of
		//        elements it invokes object.Equals( object ) which miserably fails if the elements are collections.
		//        Thus, we have to check if the values are tuples, and if so, compare them in a way that makes sense.
		if( a is SysCompiler.ITuple aTuple && b is SysCompiler.ITuple bTuple )
			return tuplesEqual( aTuple, bTuple );
		// PEARL: ImmutableArray overrides object.Equals() with the express intent of fucking it up.  So, we have to
		//        check if the left value is an ImmutableArray, and if so, perform the comparison in a way that makes
		//        sense.
		if( isImmutableArray( type ) )
			return legacyEnumerablesEqual( (Legacy.IEnumerable)a, (Legacy.IEnumerable)b );
		// PEARL: If the values are KeyValuePairs, we cannot allow KeyValuePair.Equals() to be invoked, because for
		//        each pair of elements it invokes object.Equals( object ) which miserably fails if the elements are
		//        collections.
		//        Thus, we have to check if the values are KeyValuePairs, and if so, compare them in a way that makes
		//        sense.
		if( isKeyValuePair( type ) )
			return keyValuePairsEqual( type, a, b );
		if( overridesEqualsMethod( type ) )
			goto end;
		if( a is Legacy.IEnumerable aEnumerable && b is Legacy.IEnumerable bEnumerable )
			return legacyEnumerablesEqual( aEnumerable, bEnumerable );
		if( type != b.GetType() )
			return false;
		if( DebugMode )
			reportClassWithoutEquals( type );
end:
#pragma warning disable RS0030 // Do not use banned APIs
		return a.Equals( b );
#pragma warning restore RS0030 // Do not use banned APIs

		static bool overridesEqualsMethod( Sys.Type type )
		{
			SysReflect.MethodInfo equalsMethod = type.GetMethods( SysReflect.BindingFlags.Instance | SysReflect.BindingFlags.Public ) //
					.Single( m => m.Name == "Equals" && m.GetBaseDefinition().DeclaringType == typeof( object ) );
			return equalsMethod.DeclaringType != typeof( object ) && equalsMethod.DeclaringType != typeof( Sys.ValueType );
		}

		static bool isImmutableArray( Sys.Type type )
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( ImmutableArray<> );
		}

		static bool isKeyValuePair( Sys.Type type )
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( KeyValuePair<,> );
		}
	}

	bool legacyEnumerablesEqual( Legacy.IEnumerable a, Legacy.IEnumerable b )
	{
		Legacy.IEnumerator enumerator1 = a.GetEnumerator();
		Legacy.IEnumerator enumerator2 = b.GetEnumerator();
		try
		{
			while( enumerator1.MoveNext() )
			{
				if( !enumerator2.MoveNext() )
					return false;
				if( !Equals( enumerator1.Current, enumerator2.Current ) )
					return false;
			}
			if( enumerator2.MoveNext() )
				return false;
			return true;
		}
		finally
		{
			(enumerator1 as Sys.IDisposable)?.Dispose();
			(enumerator2 as Sys.IDisposable)?.Dispose();
		}
	}

	static bool tuplesEqual( SysCompiler.ITuple aTuple, SysCompiler.ITuple bTuple )
	{
		int count = Math.Min( aTuple.Length, bTuple.Length );
		for( int i = 0; i < count; i++ )
			if( !Instance.Equals( aTuple[i], bTuple[i] ) )
				return false;
		return aTuple.Length == bTuple.Length;
	}

	(SysReflect.PropertyInfo keyPropertyInfo, SysReflect.PropertyInfo valuePropertyInfo) getKeyValuePairPropertyInfos( Sys.Type type )
	{
		Assert( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( KeyValuePair<,> ) );
		lock( keyValuePairPropertyInfos )
		{
			if( keyValuePairPropertyInfos.TryGetValue( type, out (SysReflect.PropertyInfo keyPropertyInfo, SysReflect.PropertyInfo valuePropertyInfo) existingPropertyInfos ) )
				return existingPropertyInfos;
			SysReflect.PropertyInfo keyPropertyInfo = ReflectionHelpers.GetPropertyInfo( type, "Key" );
			SysReflect.PropertyInfo valuePropertyInfo = ReflectionHelpers.GetPropertyInfo( type, "Value" );
			keyValuePairPropertyInfos.Add( type, (keyPropertyInfo, valuePropertyInfo) );
			return (keyPropertyInfo, valuePropertyInfo);
		}
	}

	bool keyValuePairsEqual( Sys.Type type, object a, object b )
	{
		(SysReflect.PropertyInfo keyPropertyInfo, SysReflect.PropertyInfo valuePropertyInfo) = getKeyValuePairPropertyInfos( type );
		object? aKey = keyPropertyInfo.GetValue( a );
		object? bKey = keyPropertyInfo.GetValue( b );
		if( !Equals( aKey, bKey ) )
			return false;
		object? aValue = valuePropertyInfo.GetValue( a );
		object? bValue = valuePropertyInfo.GetValue( a );
		if( !Equals( aValue, bValue ) )
			return false;
		return true;
	}

	void reportClassWithoutEquals( Sys.Type type )
	{
		if( type == typeof( object ) )
			return;
		if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
			return;
		lock( reportedTypes )
			if( reportedTypes.Add( type ) )
				Log.Error( $"Type {type.GetCSharpName()} does not override object.Equals() !" );
	}
}
