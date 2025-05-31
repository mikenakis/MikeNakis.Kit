namespace MikeNakis.Kit.Test;

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
	public void T201_Abs_Of_MinValue_Of_Signed_Integer_Type_Throws()
	{
		Assert( TryCatch( () => Math.Abs( sbyte.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( short.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( int.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( long.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( nint.MinValue ) ) is Sys.OverflowException );
	}
}
