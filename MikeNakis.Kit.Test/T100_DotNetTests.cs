namespace MikeNakis.Kit.Test;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using VSTesting = Microsoft.VisualStudio.TestTools.UnitTesting;

[VSTesting.TestClass]
public sealed class T100_DotNetTests
{
	[VSTesting.TestMethod]
	public void T00_Abs_Of_Int_MinValue_Throws()
	{
		Sys.Exception? caughtException = TryCatch( () => Sys.Math.Abs( int.MinValue ) );
		NotNullCast( caughtException, out Sys.OverflowException _ );
	}
}
