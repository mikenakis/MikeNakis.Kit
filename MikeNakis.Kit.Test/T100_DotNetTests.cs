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
}
