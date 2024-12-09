namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T101_KitTests
{
	[VSTesting.TestMethod]
	public void T01_SafeToString_Works()
	{
		Assert( KitHelpers.SafeToString( (int?)5 ) == "5" );
		Assert( KitHelpers.SafeToString( default( int? ) ) == "null" );
		Assert( KitHelpers.SafeToString( null ) == "null" );
		Assert( KitHelpers.SafeToString( "" ) == "\"\"" );
		Assert( KitHelpers.SafeToString( "x" ) == "\"x\"" );
		Assert( KitHelpers.SafeToString( 'x' ) == "\'x\'" );
		Assert( KitHelpers.SafeToString( "\x1a" ) == "\"\\x1a\"" );
		Assert( KitHelpers.SafeToString( "\x0a" ) == "\"\\n\"" );
		Assert( KitHelpers.SafeToString( '\x1a' ) == "'\\x1a'" );
		Assert( KitHelpers.SafeToString( '\x0a' ) == "'\\n'" );
	}

	[VSTesting.TestMethod]
	public void T02_My_Collections_Work()
	{
		Assert( createAndCompare( createMutableList ) );
		return;

		static IEnumerable<int> createMutableList() => new MutableList<int>( EnumerableOf( 1, 2, 3 ) ).AsEnumerable;
	}

	[VSTesting.TestMethod]
	public void T03_Collection_Expressions_Are_Defective()
	{
		Assert( !createAndCompare( createEnumerable ) );
		Assert( !createAndCompare( createReadOnlyCollection ) );
		Assert( !createAndCompare( createReadOnlyList ) );
		Assert( !createAndCompare( createCollection ) );
		Assert( !createAndCompare( createList ) );
		return;

		static IEnumerable<int> createEnumerable() => [1, 2, 3];
		static IReadOnlyCollection<int> createReadOnlyCollection() => [1, 2, 3];
		static IReadOnlyList<int> createReadOnlyList() => [1, 2, 3];
		static ICollection<int> createCollection() => [1, 2, 3];
		static IList<int> createList() => [1, 2, 3];
	}

	static bool createAndCompare<T>( Sys.Func<IEnumerable<T>> factory )
	{
		IEnumerable<T> enumerable1 = factory.Invoke();
		IEnumerable<T> enumerable2 = factory.Invoke();
		return enumerable1.Equals( enumerable2 );
	}

	[VSTesting.TestMethod]
	public void T04_MutableList_Works()
	{
		MutableList<int> mutableList = new();
		mutableList.Add( 1 );
		Assert( mutableList.Equals( ReadOnlyListOf( 1 ) ) );
	}

	[VSTesting.TestMethod]
	public void T05_Use_Of_Array_As_ReadOnlyList_Is_Caught()
	{
		MutableList<int> mutableList = new();
		IReadOnlyList<int> array = new[] { 1 };
		Sys.Exception? caughtException = TryCatch( () => mutableList.Equals( array ) );
		NotNullCast( caughtException, out AssertionFailureException _ );
	}

	[VSTesting.TestMethod]
	public void T05_Use_Of_Array_As_Object_Is_Caught()
	{
		MutableList<int> mutableList = new();
		object array = new[] { 1 };
		Sys.Exception? caughtException = TryCatch( () => mutableList.Equals( array ) );
		NotNullCast( caughtException, out AssertionFailureException _ );
	}
}
