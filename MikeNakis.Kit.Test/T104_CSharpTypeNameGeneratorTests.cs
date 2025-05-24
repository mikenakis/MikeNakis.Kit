#pragma warning disable IDE0161 // Convert to file-scoped namespace

namespace MikeNakis.Kit.Test //we use block-scoped namespace because at the end of this file we have some classes in the global namespace.
{
	using MikeNakis.Kit.Extensions;
	using CodeDom = Sys.CodeDom;
	using CSharp = Microsoft.CSharp;
	using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

	[VSTesting.TestClass]
	public sealed class T104_CSharpTypeNameGeneratorTests
	{
		[VSTesting.TestMethod]
		public void CSharpTypeNameGenerator_Test()
		{
			var cSharpCompiler = new CSharp.CSharpCodeProvider();

			test( typeof( sbyte ) );
			test( typeof( byte ) );
			test( typeof( short ) );
			test( typeof( ushort ) );
			test( typeof( int ) );
			test( typeof( uint ) );
			test( typeof( long ) );
			test( typeof( ulong ) );
			test( typeof( char ) );
			test( typeof( float ) );
			test( typeof( double ) );
			test( typeof( bool ) );
			test( typeof( decimal ) );

			test( typeof( object ) );
			test( typeof( string ) );
			test( typeof( Sys.Int128 ) );
			test( typeof( Sys.Guid ) );

			test( typeof( int[] ) );
			test( typeof( int[,] ) );
			test( typeof( int[,,] ) );
			test( typeof( int[][] ) );

			test( typeof( int? ) );
			test( typeof( int?[] ) );
			test( typeof( int?[,] ) );
			test( typeof( int?[,,] ) );
			test( typeof( int?[][] ) );

			test( typeof( List<> ) );
			test( typeof( List<int> ) );
			test( typeof( List<int[,]> ) );
			test( typeof( List<int[][]> ) );
			test( typeof( List<int?> ) );
			test( typeof( List<int?[,]> ) );
			test( typeof( List<int?[][]> ) );
			test( typeof( List<int>[] ) );
			test( typeof( List<int>[,] ) );
			test( typeof( List<int>[][] ) );
			test( typeof( List<List<int>> ) );
			test( typeof( List<List<int[,]>> ) );
			test( typeof( List<List<int[][]>> ) );
			test( typeof( List<List<int?>> ) );
			test( typeof( List<List<int?[,]>> ) );
			test( typeof( List<List<int?[][]>> ) );
			test( typeof( Dictionary<,> ) );
			test( typeof( Dictionary<int, bool> ) );
			test( typeof( Dictionary<List<int>, List<bool>> ) );
			test( typeof( List<>.Enumerator ) );
			test( typeof( List<int>.Enumerator ) );
			test( typeof( List<List<int>>.Enumerator ) );
			test( typeof( Dictionary<,>.Enumerator ) );
			test( typeof( Dictionary<int, bool>.Enumerator ) );
			test( typeof( Dictionary<List<int>, List<bool>>.Enumerator ) );

			test( typeof( C0<int>.C1A ) );
			test( typeof( C0<int>.C1B<bool, byte> ) );
			test( typeof( C0<int>.C1B<bool, byte>.C2A ) );
			test( typeof( C0<int>.C1B<bool, byte>.C2B<char> ) );

			test( typeof( C0<>.C1A ) );
			test( typeof( C0<>.C1B<,> ) );
			test( typeof( C0<>.C1B<,>.C2A ) );
			test( typeof( C0<>.C1B<,>.C2B<> ) );

			test( typeof( C0<> ).GetField( nameof( C0<int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1A ).GetField( nameof( C0<int>.C1A.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,> ).GetField( nameof( C0<int>.C1B<int, int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,> ).GetField( nameof( C0<int>.C1B<int, int>.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,> ).GetField( nameof( C0<int>.C1B<int, int>.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2B<> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2B<> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2B<> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<>.C1B<,>.C2B<> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F4 ) ).OrThrow().FieldType );

			test( typeof( C0<bool> ).GetField( nameof( C0<int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1A ).GetField( nameof( C0<int>.C1A.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char> ).GetField( nameof( C0<int>.C1B<int, int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char> ).GetField( nameof( C0<int>.C1B<int, int>.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char> ).GetField( nameof( C0<int>.C1B<int, int>.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2A ).GetField( nameof( C0<int>.C1B<int, int>.C2A.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2B<long> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F1 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2B<long> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F2 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2B<long> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F3 ) ).OrThrow().FieldType );
			test( typeof( C0<bool>.C1B<byte, char>.C2B<long> ).GetField( nameof( C0<int>.C1B<int, int>.C2B<int>.F4 ) ).OrThrow().FieldType );

			void test( Sys.Type type )
			{
				string generatedTypeName = type.GetCSharpName();
				string expectedTypeName = getTypeNameFromCSharpCompiler( type, cSharpCompiler );
				Assert( generatedTypeName == expectedTypeName );
			}
		}

		static string getTypeNameFromCSharpCompiler( Sys.Type type, CSharp.CSharpCodeProvider cSharpCompiler )
		{
			var typeRef = new CodeDom.CodeTypeReference( type );
			string typeName = cSharpCompiler.GetTypeOutput( typeRef );
			//PEARL: Microsoft.CSharp.CSharpCodeProvider has a bug where it includes superfluous spaces in the generated type names. We remove them here.
			return typeName.Replace2( " ", "" );
		}
	}
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable CA1852 // Type '{}' can be sealed because...

class C0<T1>
{
	public T1 F1 = default!;

	public class C1A
	{
		public T1 F1 = default!;
	}

	public class C1B<T2, T3>
	{
		public T1 F1 = default!;
		public T2 F2 = default!;
		public T3 F3 = default!;

		public class C2A
		{
			public T1 F1 = default!;
			public T2 F2 = default!;
			public T3 F3 = default!;
		}

		public class C2B<T4>
		{
			public T1 F1 = default!;
			public T2 F2 = default!;
			public T3 F3 = default!;
			public T4 F4 = default!;
		}
	}
}
