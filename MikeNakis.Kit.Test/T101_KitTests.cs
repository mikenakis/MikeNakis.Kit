namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
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

	[VSTesting.TestMethod]
	public void T09_RoundSignificantDigits_Works()
	{
		check( 0.000149, 1, (0.0001, 4) );
		check( 0.000151, 1, (0.0002, 4) );
		check( 0.00149, 1, (0.001, 3) );
		check( 0.00150, 1, (0.002, 3) );
		check( 0.0149, 1, (0.01, 2) );
		check( 0.0150, 1, (0.02, 2) );
		check( 0.149, 1, (0.1, 1) );
		check( 0.150, 1, (0.2, 1) );
		check( 1.49, 1, (1.0, 0) );
		check( 1.50, 1, (2.0, 0) );
		check( 14.0, 1, (10.0, -1) );
		check( 15.0, 1, (20.0, -1) );
		check( 149.0, 1, (100.0, -2) );
		check( 150.0, 1, (200.0, -2) );
		check( 1499.0, 1, (1000.0, -3) );
		check( 1500.0, 1, (2000.0, -3) );

		check( 0.0001249, 2, (0.00012, 5) );
		check( 0.0001251, 2, (0.00013, 5) );
		check( 0.001249, 2, (0.0012, 4) );
		check( 0.001250, 2, (0.0013, 4) );
		check( 0.01249, 2, (0.012, 3) );
		check( 0.01250, 2, (0.013, 3) );
		check( 0.1249, 2, (0.12, 2) );
		check( 0.1250, 2, (0.13, 2) );
		check( 1.249, 2, (1.2, 1) );
		check( 1.250, 2, (1.3, 1) );
		check( 12.49, 2, (12.0, 0) );
		check( 12.50, 2, (13.0, 0) );
		check( 124.9, 2, (120.0, -1) );
		check( 125.0, 2, (130.0, -1) );
		check( 1249.0, 2, (1200.0, -2) );
		check( 1250.0, 2, (1300.0, -2) );
		check( 12490.0, 2, (12000.0, -3) );
		check( 12500.0, 2, (13000.0, -3) );

		check( 0.00012349, 3, (0.000123, 6) );
		check( 0.00012351, 3, (0.000124, 6) );
		check( 0.0012349, 3, (0.00123, 5) );
		check( 0.0012350, 3, (0.00124, 5) );
		check( 0.012349, 3, (0.0123, 4) );
		check( 0.012350, 3, (0.0124, 4) );
		check( 0.12349, 3, (0.123, 3) );
		check( 0.12350, 3, (0.124, 3) );
		check( 1.2349, 3, (1.23, 2) );
		check( 1.2350, 3, (1.24, 2) );
		check( 12.349, 3, (12.3, 1) );
		check( 12.350, 3, (12.4, 1) );
		check( 123.49, 3, (123.0, 0) );
		check( 123.50, 3, (124.0, 0) );
		check( 1234.9, 3, (1230.0, -1) );
		check( 1235.0, 3, (1240.0, -1) );
		check( 12349.0, 3, (12300.0, -2) );
		check( 12350.0, 3, (12400.0, -2) );
		check( 123490.0, 3, (123000.0, -3) );
		check( 123500.0, 3, (124000.0, -3) );

		return;

		static void check( double value, int significantDigits, (double value, int roundingPosition) expectedResult )
		{
			(double value, int roundingPosition) actualResult = KitHelpers.RoundSignificantDigits( value, significantDigits );
			Assert( actualResult == expectedResult );
		}
	}

	[VSTesting.TestMethod]
	public void T10_DoubleToString_Works()
	{
		check( 0.000149, 1, "0.0001" );
		check( 0.000151, 1, "0.0002" );
		check( 0.00149, 1, "0.001" );
		check( 0.00150, 1, "0.002" );
		check( 0.0149, 1, "0.01" );
		check( 0.0150, 1, "0.02" );
		check( 0.149, 1, "0.1" );
		check( 0.150, 1, "0.2" );
		check( 1.49, 1, "1" );
		check( 1.50, 1, "2" );
		check( 14.0, 1, "10" );
		check( 15.0, 1, "20" );
		check( 149.0, 1, "100" );
		check( 150.0, 1, "200" );
		check( 1499.0, 1, "1000" );
		check( 1500.0, 1, "2000" );

		check( 0.0001249, 2, "0.00012" );
		check( 0.0001251, 2, "0.00013" );
		check( 0.001249, 2, "0.0012" );
		check( 0.001250, 2, "0.0013" );
		check( 0.01249, 2, "0.012" );
		check( 0.01250, 2, "0.013" );
		check( 0.1249, 2, "0.12" );
		check( 0.1250, 2, "0.13" );
		check( 1.249, 2, "1.2" );
		check( 1.250, 2, "1.3" );
		check( 12.49, 2, "12" );
		check( 12.50, 2, "13" );
		check( 124.9, 2, "120" );
		check( 125.0, 2, "130" );
		check( 1249.0, 2, "1200" );
		check( 1250.0, 2, "1300" );
		check( 12490.0, 2, "12000" );
		check( 12500.0, 2, "13000" );

		check( 0.00012349, 3, "0.000123" );
		check( 0.00012351, 3, "0.000124" );
		check( 0.0012349, 3, "0.00123" );
		check( 0.0012350, 3, "0.00124" );
		check( 0.012349, 3, "0.0123" );
		check( 0.012350, 3, "0.0124" );
		check( 0.12349, 3, "0.123" );
		check( 0.12350, 3, "0.124" );
		check( 1.2349, 3, "1.23" );
		check( 1.2350, 3, "1.24" );
		check( 12.349, 3, "12.3" );
		check( 12.350, 3, "12.4" );
		check( 123.49, 3, "123" );
		check( 123.50, 3, "124" );
		check( 1234.9, 3, "1230" );
		check( 1235.0, 3, "1240" );
		check( 12349.0, 3, "12300" );
		check( 12350.0, 3, "12400" );
		check( 123490.0, 3, "123000" );
		check( 123500.0, 3, "124000" );

		check( 9.96, 2, "10" ); // to 2 figures yields 10.0 instead of 10

		return;

		static void check( double value, int significantDigits, string expectedResult )
		{
			string actualResult = KitHelpers.ToString( value, significantDigits );
			Assert( actualResult == expectedResult );
		}
	}
}
