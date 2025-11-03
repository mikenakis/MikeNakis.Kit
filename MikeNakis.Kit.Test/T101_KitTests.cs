namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using Sys = System;
using SysText = System.Text;
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
		Assert( KitHelpers.SafeToString( "\x1a" ) == "\"\\u001a\"" );
		Assert( KitHelpers.SafeToString( "\x0a" ) == "\"\\n\"" );
		Assert( KitHelpers.SafeToString( '\x1a' ) == "'\\u001a'" );
		Assert( KitHelpers.SafeToString( '\x0a' ) == "'\\n'" );
		Assert( KitHelpers.SafeToString( typeof( int ) ) == "int" );
		Assert( KitHelpers.SafeToString( typeof( Sys.DateTime ) ) == "struct System.DateTime" );
	}

	[VSTesting.TestMethod]
	public void T02_My_MutableList_Equals_Works()
	{
		IEnumerable<int> enumerable1 = createMutableList();
		IEnumerable<int> enumerable2 = createMutableList();
		Assert( enumerable1.Equals( enumerable2 ) );
		return;

		static IEnumerable<int> createMutableList() => new MutableList<int>( EnumerableOf( 1, 2, 3 ) ).AsEnumerable;
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
		Sys.Exception? exception = TryCatch( () => mutableList.Equals( array ) );
		Assert( exception is AssertionFailureException );
	}

	[VSTesting.TestMethod]
	public void T05_Use_Of_Array_As_Object_Is_Caught()
	{
		MutableList<int> mutableList = new();
		object array = new[] { 1 };
		Sys.Exception? exception = TryCatch( () => mutableList.Equals( array ) );
		Assert( exception is AssertionFailureException );
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

	[VSTesting.TestMethod]
	public void T07_IsPrime_Works()
	{
		bool[] primeFlags = new bool[10000];
		sieveOfEratosthenes( primeFlags );
		for( int i = 0; i < primeFlags.Length; i++ )
		{
			bool isPrime = KitHelpers.IsPrime( i );
			Assert( isPrime == primeFlags[i] );
		}

		//from https://stackoverflow.com/a/1043247/773113
		static void sieveOfEratosthenes( bool[] primeFlags )
		{
			primeFlags[0] = false;
			primeFlags[1] = false;
			for( int i = 2; i < primeFlags.Length; i++ )
				primeFlags[i] = true;
			for( int i = 0; i * i < primeFlags.Length; i++ )
				if( primeFlags[i] )
					for( int j = i * i; j < primeFlags.Length; j += i )
						primeFlags[j] = false;
		}
	}

	[VSTesting.TestMethod]
	public void T08_SafeSubstring_Works()
	{
		const char ellipsis = '\u2026'; //Unicode "Horizontal Ellipsis"

		Assert( "0123456789".SafeSubstring( 0 ) == "0123456789" );
		Assert( "0123456789".SafeSubstring( 1 ) == "123456789" );
		Assert( "0123456789".SafeSubstring( 9 ) == "9" );
		Assert( "0123456789".SafeSubstring( 10 ) == "" );
		Assert( "0123456789".SafeSubstring( 11 ) == "" );

		Assert( "0123456789".SafeSubstring( 0, 0 ) == "" );
		Assert( "0123456789".SafeSubstring( 0, 1 ) == "0" );
		Assert( "0123456789".SafeSubstring( 0, 2 ) == "01" );
		Assert( "0123456789".SafeSubstring( 0, 9 ) == "012345678" );
		Assert( "0123456789".SafeSubstring( 0, 10 ) == "0123456789" );
		Assert( "0123456789".SafeSubstring( 0, 11 ) == "0123456789" );

		Assert( "0123456789".SafeSubstring( 1, 0 ) == "" );
		Assert( "0123456789".SafeSubstring( 1, 1 ) == "1" );
		Assert( "0123456789".SafeSubstring( 1, 2 ) == "12" );
		Assert( "0123456789".SafeSubstring( 1, 9 ) == "123456789" );
		Assert( "0123456789".SafeSubstring( 1, 10 ) == "123456789" );

		Assert( "0123456789".SafeSubstring( 8, 0 ) == "" );
		Assert( "0123456789".SafeSubstring( 8, 1 ) == "8" );
		Assert( "0123456789".SafeSubstring( 8, 2 ) == "89" );
		Assert( "0123456789".SafeSubstring( 8, 3 ) == "89" );

		Assert( "0123456789".SafeSubstring( 9, 0 ) == "" );
		Assert( "0123456789".SafeSubstring( 9, 1 ) == "9" );
		Assert( "0123456789".SafeSubstring( 9, 2 ) == "9" );

		Assert( "0123456789".SafeSubstring( 10, 0 ) == "" );
		Assert( "0123456789".SafeSubstring( 10, 1 ) == "" );

		Assert( "0123456789".SafeSubstring( 0, 0, true ) == "" );
		Assert( "0123456789".SafeSubstring( 0, 1, true ) == $"{ellipsis}" );
		Assert( "0123456789".SafeSubstring( 0, 2, true ) == "0…" );
		Assert( "0123456789".SafeSubstring( 0, 9, true ) == "01234567…" );
		Assert( "0123456789".SafeSubstring( 0, 10, true ) == "0123456789" );
		Assert( "0123456789".SafeSubstring( 0, 11, true ) == "0123456789" );

		Assert( "0123456789".SafeSubstring( 1, 0, true ) == "" );
		Assert( "0123456789".SafeSubstring( 1, 1, true ) == $"{ellipsis}" );
		Assert( "0123456789".SafeSubstring( 1, 2, true ) == $"1{ellipsis}" );
		Assert( "0123456789".SafeSubstring( 1, 9, true ) == "123456789" );
		Assert( "0123456789".SafeSubstring( 1, 10, true ) == "123456789" );

		Assert( "0123456789".SafeSubstring( 8, 0, true ) == "" );
		Assert( "0123456789".SafeSubstring( 8, 1, true ) == $"{ellipsis}" );
		Assert( "0123456789".SafeSubstring( 8, 2, true ) == "89" );
		Assert( "0123456789".SafeSubstring( 8, 3, true ) == "89" );

		Assert( "0123456789".SafeSubstring( 9, 0, true ) == "" );
		Assert( "0123456789".SafeSubstring( 9, 1, true ) == "9" );
		Assert( "0123456789".SafeSubstring( 9, 2, true ) == "9" );

		Assert( "0123456789".SafeSubstring( 10, 0, true ) == "" );
		Assert( "0123456789".SafeSubstring( 10, 1, true ) == "" );
	}
}
