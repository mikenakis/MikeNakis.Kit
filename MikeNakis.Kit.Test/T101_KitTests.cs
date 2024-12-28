namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;
using SysText = System.Text;

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

	[VSTesting.TestMethod]
	public void T06_IsPrintable_Works()
	{
		bool[] shouldBePrintable = new bool[65536];
		foreach( char c in getAllCharactersOfWindows1252Encoding() )
			shouldBePrintable[c] = true;
		for( int i = 0; i < shouldBePrintable.Length; i++ )
			Assert( shouldBePrintable[i] == KitHelpers.IsPrintable( (char)i ) );

		static string getAllCharactersOfWindows1252Encoding()
		{
			byte[] windows1252bytes = getAllBytesOfWindows1252Encoding();
			SysText.Encoding.RegisterProvider( SysText.CodePagesEncodingProvider.Instance );
			SysText.Encoding windows1252encoding = SysText.Encoding.GetEncoding( 1252 );
			return windows1252encoding.GetString( windows1252bytes );

			static byte[] getAllBytesOfWindows1252Encoding()
			{
				byte[] windows1252bytes = new byte[217];
				int n = 0;
				for( int i = 32; i < 256; i++ )
				{
					if( i is 0x7f or 0x81 or 0x8D or 0x8F or 0x90 or 0x9D or 0xAD )
						continue;
					windows1252bytes[n++] = (byte)i;
				}
				Assert( n == windows1252bytes.Length );
				return windows1252bytes;
			}
		}
	}
}
