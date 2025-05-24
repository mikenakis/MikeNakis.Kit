namespace MikeNakis.Kit.Test;

using MikeNakis.Kit.Extensions;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T100_DotNetTests
{
	[VSTesting.TestMethod]
	public void T00_Abs_Of_Int_MinValue_Throws()
	{
		Sys.Exception? exception = TryCatch( () => Math.Abs( int.MinValue ) );
		Assert( exception.OrThrow() is Sys.OverflowException );
	}

	[VSTesting.TestMethod]
	public void T01_Generic_Type_Definitions_Are_What_They_Are()
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
	public void T01_Generic_Types_Are_What_They_Are()
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
}
