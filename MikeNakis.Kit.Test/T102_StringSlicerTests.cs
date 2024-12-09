namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T102_StringSlicerTests
{
	[VSTesting.TestMethod]
	public void T01_String_Slicing_Works()
	{
		test( "", ' ' );
		test( " ", ' ' );
		test( "a", ' ' );
		test( " a", ' ' );
		test( "a ", ' ' );
		test( " a ", ' ' );
		test( "a b", ' ' );
		test( " a b", ' ' );
		test( "a b ", ' ' );
		test( " a b ", ' ' );
		test( "  a  b  ", ' ' );
	}

	static void test( string s, char delimiter )
	{
		string[] parts = s.Split( delimiter );
		StringSlicer stringSlicer = s.Slice( delimiter );
		IReadOnlyList<(int start, int end)> ranges = select( stringSlicer );
		IReadOnlyList<string> slices = ranges.Select( range => s[range.start..range.end] );
		Assert( slices.SequenceEqual( parts ) );
	}

	static IReadOnlyList<(int start, int end)> select( StringSlicer stringSlicer )
	{
		MutableList<(int start, int end)> mutableRanges = new();
		for( StringSlicer.Traverser traverser = stringSlicer.GetTraverser(); traverser.HasCurrent; traverser.MoveNext() )
			mutableRanges.Add( traverser.Current );
		return mutableRanges.Collect();
	}
}
