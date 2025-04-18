namespace MikeNakis.Kit;

using Sys = System;
using MikeNakis.Kit.Extensions;
using SysComp = System.Runtime.CompilerServices;

#pragma warning disable CA2201 // Do not raise reserved exception types

public static class SolutionSourcePath
{
	static string? lazyValue;
	public static string Value => lazyValue ??= calculateSolutionSourcePath();

	// NOTE: this function is invoked from the logging subsystem, so it must refrain from causing anything to be logged,
	//       otherwise there will be a StackOverflowException.
	static string calculateSolutionSourcePath()
	{
		string myRelativePath = "MikeNakis.Kit\\" + typeof( SolutionSourcePath ).Name + ".cs";
		string sourceFileName = getSourceFileName();
		if( sourceFileName.EndsWith2( myRelativePath ) )
			sourceFileName = sourceFileName[..^myRelativePath.Length];
		return sourceFileName;
	}

	static string getSourceFileName( [SysComp.CallerFilePath] string? sourceFileName = null )
	{
		if( sourceFileName == null )
			throw new Sys.Exception();
		return sourceFileName;
	}
}
