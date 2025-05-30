namespace MikeNakis.Kit.Test;

using MikeNakis.Kit.Extensions;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T000_FundamentalTests
{
	[VSTesting.TestMethod]
	public void T001_Nullable_Reference_Types_Are_A_Bit_Retarded()
	{
		// no warning when passing non-nullable to function accepting nullable.
		string nonNullable = Identity( "" );
		test( nonNullable );

		// no warning when checking whether non-nullable is null.
		if( nonNullable == null )
			throw new AssertionFailureException();

		// no warning when assigning non-nullable to nullable.
		string? nullable = Identity( nonNullable );
		Identity( nullable );

		static void test( string? nullable )
		{
			Identity( nullable );
		}
	}

	[VSTesting.TestMethod]
	public void T101_Generic_Type_Definitions_Are_What_They_Are()
	{
		Sys.Type genericTypeDefinition = typeof( IEnumerable<> );
		Assert( genericTypeDefinition.IsGenericType );
		Assert( genericTypeDefinition.IsGenericTypeDefinition );
		Assert( genericTypeDefinition.ContainsGenericParameters );
		Assert( !genericTypeDefinition.IsConstructedGenericType );
		Assert( genericTypeDefinition.ReferenceEquals( genericTypeDefinition.GetGenericTypeDefinition() ) );
		Assert( genericTypeDefinition.GenericTypeArguments.Length == 0 );
		Assert( genericTypeDefinition.GetGenericArguments().Length == 1 );
		Sys.Type genericTypeArgument = genericTypeDefinition.GetGenericArguments()[0];
		Assert( !genericTypeArgument.IsGenericType );
		Assert( !genericTypeArgument.IsConstructedGenericType );
		Assert( genericTypeArgument.ContainsGenericParameters );
		Assert( !genericTypeArgument.IsGenericTypeDefinition );
		Assert( genericTypeArgument.IsGenericParameter );
		Assert( genericTypeArgument.IsGenericTypeParameter );
		Assert( !genericTypeArgument.IsGenericMethodParameter );
		Assert( genericTypeArgument.Name == "T" );
		Assert( genericTypeArgument.FullName == null );
	}

	[VSTesting.TestMethod]
	public void T102_Generic_Types_Are_What_They_Are()
	{
		Sys.Type genericType = typeof( IEnumerable<string> );
		Assert( genericType.IsGenericType );
		Assert( !genericType.IsGenericTypeDefinition );
		Assert( genericType.GetGenericTypeDefinition().ReferenceEquals( typeof( IEnumerable<> ) ) );
		Assert( !genericType.ContainsGenericParameters );
		Assert( genericType.IsConstructedGenericType );
		Assert( Enumerable.SequenceEqual( genericType.GenericTypeArguments, genericType.GetGenericArguments() ) );
		Assert( genericType.GetGenericArguments().Length == 1 );
		Sys.Type genericArgument = genericType.GetGenericArguments()[0];
		Assert( !genericArgument.IsGenericType );
		Assert( !genericArgument.IsConstructedGenericType );
		Assert( !genericArgument.ContainsGenericParameters );
		Assert( !genericArgument.IsGenericTypeDefinition );
		Assert( !genericArgument.IsGenericParameter );
		Assert( !genericArgument.IsGenericTypeParameter );
		Assert( !genericArgument.IsGenericMethodParameter );
		Assert( genericArgument == typeof( string ) );
	}

	[VSTesting.TestMethod]
	public void T201_Abs_Of_MinValue_Of_Signed_Integer_Type_Throws()
	{
		Assert( TryCatch( () => Math.Abs( sbyte.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( short.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( int.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( long.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( nint.MinValue ) ) is Sys.OverflowException );
	}
}
