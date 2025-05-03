namespace MikeNakis.Kit.Test;

using MikeNakis.Kit.Collections;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T103_CircularListTests
{
	[VSTesting.TestMethod]
	public void SimpleTest()
	{
		CircularList<int> list = new();
		list.Add( 3 );
		Assert( list[0] == 3 );
		list.Add( 4 );
		Assert( list[1] == 4 );
		Assert( list.IndexOf( 3 ) == 0 );
		list.Remove( 3 );
		Assert( list.Count == 1 );
		Assert( list[0] == 4 );
	}

	[VSTesting.TestMethod]
	public void RemoveAtTest()
	{
		CircularList<int> list = new( 2 );
		for( int i = 0; i < 8; i++ )
			list.Add( i );
		list.RemoveAt( 5 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 1, 2, 3, 4, 6, 7 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 2, 3, 4, 6, 7 ) ) );
		list.RemoveAt( 3 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 2, 3, 6, 7 ) ) );
		list.RemoveAt( 2 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 2, 6, 7 ) ) );
		list.RemoveAt( 2 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 2, 7 ) ) );
		list.RemoveAt( 2 );
		Assert( list.SequenceEqual( EnumerableOf( 0, 2 ) ) );
		list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 2 ) ) );
		list.RemoveAt( 0 );
		int value = 0;
		Sys.Exception? thrownException = TryCatch( () => value = list[0] );
		Assert( thrownException is AssertionFailureException );
	}

	[VSTesting.TestMethod]
	public void RemoveAtTest2()
	{
		CircularList<int> list = new( 2 );
		for( int i = 0; i < 8; i++ )
			list.Add( i );
		Assert( list.SequenceEqual( EnumerableOf( 0, 1, 2, 3, 4, 5, 6, 7 ) ) );
		for( int i = 0; i < 6; i++ )
			list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 7 ) ) );
		list.Add( 8 );
		list.Add( 9 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 7, 8, 9 ) ) );
		list.RemoveAt( 2 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 7, 9 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 9 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 6 ) ) );
		Sys.Exception? thrownException = TryCatch( () => list.RemoveAt( 1 ) );
		Assert( thrownException is AssertionFailureException );
		list.RemoveAt( 0 );
		int value = 0;
		thrownException = TryCatch( () => value = list[0] );
		Assert( thrownException is AssertionFailureException );
	}

	[VSTesting.TestMethod]
	public void RemoveAtTest3()
	{
		CircularList<int> list = new( 2 );
		for( int i = 0; i < 8; i++ )
			list.Add( i );
		for( int i = 0; i < 6; i++ )
			list.RemoveAt( 0 );
		list.Add( 8 );
		list.Add( 9 );
		list.Add( 10 );
		list.Add( 11 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 7, 8, 9, 10, 11 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 8, 9, 10, 11 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 6, 9, 10, 11 ) ) );
		list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 9, 10, 11 ) ) );
		list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 10, 11 ) ) );
		list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 11 ) ) );
	}

	[VSTesting.TestMethod]
	public void RemoveAtTest4()
	{
		CircularList<int> list = new( 2 );
		for( int i = 0; i < 8; i++ )
			list.Add( i );
		for( int i = 0; i < 7; i++ )
			list.RemoveAt( 0 );
		list.Add( 8 );
		list.Add( 9 );
		list.Add( 10 );
		list.Add( 11 );
		list.Add( 12 );
		Assert( list.SequenceEqual( EnumerableOf( 7, 8, 9, 10, 11, 12 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 7, 9, 10, 11, 12 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 7, 10, 11, 12 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 7, 11, 12 ) ) );
		list.RemoveAt( 0 );
		Assert( list.SequenceEqual( EnumerableOf( 11, 12 ) ) );
		list.RemoveAt( 1 );
		Assert( list.SequenceEqual( EnumerableOf( 11 ) ) );
	}

	[VSTesting.Ignore] //too slow
	[VSTesting.TestMethod]
	public void RandomInsertTest()
	{
		int count = 12345;
		MutableList<int> refList = new( 33 );
		CircularList<int> list = new( 33 );
		setupRandomTest( refList, list, count );

		Sys.Random random = new( 3214 );
		for( int i = 0; i < count; ++i )
		{
			int index = random.Next( refList.Count );
			int value = random.Next();

			list.Insert( index, value );
			refList.Insert( index, value );

			try
			{
				Assert( refList.SequenceEqual( list ) );
			}
			catch( Sys.Exception )
			{
				SysConsole.WriteLine( $"Case: i={i},index={index},value={value}" );
				printResult( refList, list );
				throw;
			}
		}
	}

	[VSTesting.Ignore] //too slow
	[VSTesting.TestMethod]
	public void RandomRemoveTest()
	{
		int count = 10248;
		MutableList<int> refList = new( 33 );
		CircularList<int> list = new( 33 );
		setupRandomTest( refList, list, count );

		Sys.Random random = new( 3456345 );
		for( int i = 0; i < count; ++i )
		{
			int index = random.Next( refList.Count );
			int value = random.Next();

			list.RemoveAt( index );
			refList.RemoveAt( index );

			try
			{
				Assert( refList.SequenceEqual( list ) );
			}
			catch( Sys.Exception )
			{
				SysConsole.WriteLine( $"Case: i={i},index={index},value={value}" );
				printResult( refList, list );
				throw;
			}
		}
	}

	[VSTesting.Ignore] //too slow
	[VSTesting.TestMethod]
	public void RandomInsertRemoveTest()
	{
		int count = 20248;
		MutableList<int> refList = new( 33 );
		CircularList<int> list = new( 33 );
		setupRandomTest( refList, list, count );
		Sys.Random random = new( 123456 );
		for( int i = 0; i < count; ++i )
		{
			ListAction action = (ListAction)random.Next( 3 );
			int index = random.Next( refList.Count );
			int value = random.Next();
			if( action == ListAction.REMOVE_AT )
			{
				list.RemoveAt( index );
				refList.RemoveAt( index );
			}
			else if( action == ListAction.REPLACE )
			{
				list[index] = value;
				refList[index] = value;
			}
			else//insert
			{
				list.Insert( index, value );
				refList.Insert( index, value );
			}
			try
			{
				Assert( refList.SequenceEqual( list ) );
			}
			catch( Sys.Exception )
			{
				SysConsole.WriteLine( $"Case: i={i},action={action},index={index},value={value}" );
				printResult( refList, list );
				throw;
			}
		}
	}

	static void setupRandomTest( MutableList<int> refList, CircularList<int> list, int count )
	{
		Sys.Random random = new( 123456 );

		for( int i = 0; i < count; ++i )
		{
			int value = random.Next();
			list.Add( value );
			refList.Add( value );
		}

		for( int i = 0; i < count / 2; ++i )
		{
			list.RemoveAt( 0 );
			refList.RemoveAt( 0 );
		}

		for( int i = 0; i < count / 2; ++i )
		{
			int value = random.Next();
			list.Add( value );
			refList.Add( value );
		}

		try
		{
			Assert( refList.SequenceEqual( list ) );
		}
		catch( Sys.Exception )
		{
			printResult( refList, list );
			throw;
		}
	}

	static void printResult( MutableList<int> refList, CircularList<int> list )
	{
		SysConsole.WriteLine( $"Expected Count:{refList.Count}, Actual Count:{list.Count} " );
		int count = 0;
		for( int i = 0; i < Sys.Math.Min( list.Count, refList.Count ); i++ )
		{
			if( list[i] != refList[i] )
			{
				SysConsole.WriteLine( $"At index {i}, Expected Value:{refList[i]}, Actual Value:{list[i]}" );
				count++;
				if( count >= 10 )
				{
					SysConsole.WriteLine( "...." );
					break;
				}
			}
		}
	}

	internal enum ListAction
	{
		REMOVE_AT,
		REPLACE,
		INSERT
	}
}
