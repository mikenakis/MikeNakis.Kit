namespace MikeNakis.Kit;

using Sys = System;
using MikeNakis.Kit.Extensions;
using SysComp = System.Runtime.CompilerServices;

#pragma warning disable CA2201 // Do not raise reserved exception types

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
			throw new Sys.Exception( sourceFileName );
		return sourceFileName[..^myRelativePath.Length];
	}

	static string getSourceFileName( [SysComp.CallerFilePath] string? sourceFileName = null )
	{
		if( sourceFileName == null )
			throw new Sys.Exception();
		return sourceFileName;
	}
}
