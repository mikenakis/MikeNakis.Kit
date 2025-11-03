namespace MikeNakis.Kit.Test;

using System.Collections.Generic;
using Math = System.Math;
using Sys = System;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T000_FundamentalTests
{
	[VSTesting.TestMethod]
	public void T001_Nullable_Reference_Types_Are_A_Bit_Retarded()
	{
		// no warning when passing non-nullable to function accepting nullable.
		string nonNullable = Identity( "" );
		functionAcceptingNullable( nonNullable );

		// no warning when checking whether non-nullable is null.
		if( nonNullable == null )
			throw new AssertionFailureException();

		// no warning when assigning non-nullable to nullable.
		string? nullable = nonNullable;
		Identity( nullable );

		static void functionAcceptingNullable( string? nullable )
		{
			Identity( nullable );
		}
	}

	[VSTesting.TestMethod]
	public void T002_Abs_Of_MinValue_Of_Signed_Integer_Type_Throws()
	{
		Assert( TryCatch( () => Math.Abs( sbyte.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( short.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( int.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( long.MinValue ) ) is Sys.OverflowException );
		Assert( TryCatch( () => Math.Abs( nint.MinValue ) ) is Sys.OverflowException );
	}

	[VSTesting.TestMethod]
	public void T003_Collection_Expressions_Are_Defective()
	{
		Assert( createArray().Equals( createArray() ) == false ); //unfortunately OK due to the language definition
		Assert( createEnumerable().Equals( createEnumerable() ) == false ); //defective
		Assert( createReadOnlyCollection().Equals( createReadOnlyCollection() ) == false ); //defective
		Assert( createReadOnlyList().Equals( createReadOnlyList() ) == false ); //defective
		Assert( createCollection().Equals( createCollection() ) == false ); //defective
		Assert( createList().Equals( createList() ) == false ); //defective
		return;

		static int[] createArray() => [1, 2, 3];
		static IEnumerable<int> createEnumerable() => [1, 2, 3];
		static IReadOnlyCollection<int> createReadOnlyCollection() => [1, 2, 3];
		static IReadOnlyList<int> createReadOnlyList() => [1, 2, 3];
		static ICollection<int> createCollection() => [1, 2, 3];
		static IList<int> createList() => [1, 2, 3];
	}

	struct ValueType
	{
		public int Field;

		public ValueType( int value )
		{
			Field = value;
		}

		public void IncrementField()
		{
			Field++;
		}
	}

	[VSTesting.TestMethod]
	public void T004_Value_Types_Are_For_Most_Purposes_Immutable()
	{
		ValueType instance = new( 10 );

		ValueType anotherInstance = instance;
		anotherInstance.IncrementField();
		Assert( anotherInstance.Field == 11 );
		Assert( instance.Field == 10 );

		object boxedInstance = instance;
		ValueType unboxedInstance = (ValueType)boxedInstance;
		unboxedInstance.IncrementField();
		Assert( unboxedInstance.Field == 11 );
		Assert( instance.Field == 10 );

		ValueType[] array = new[] { instance };
		instance.IncrementField();
		Assert( instance.Field == 11 );
		Assert( array[0].Field == 10 );
	}
}
