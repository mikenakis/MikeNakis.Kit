namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;
using SysComp = System.Runtime.CompilerServices;

public static class SolutionSourcePath
{
	const string myRelativePath = "MikeNakis.Kit\\" + nameof( SolutionSourcePath ) + ".cs";
	static string? lazyValue;
	public static string Value => lazyValue ??= calculateSolutionSourcePath();

	// NOTE: this function is invoked from the logging subsystem, so it must refrain from causing anything to be logged,
	//       otherwise there will be a StackOverflowException.
	static string calculateSolutionSourcePath()
	{
		string sourceFileName = getSourceFileName();
		if( !sourceFileName.EndsWith2( myRelativePath ) )
			throw new AssertionFailureException( sourceFileName );
		return sourceFileName[..^myRelativePath.Length];
	}

	static string getSourceFileName( [SysComp.CallerFilePath] string? sourceFileName = null )
	{
		if( sourceFileName == null )
			throw new AssertionFailureException();
		return sourceFileName;
	}
}
