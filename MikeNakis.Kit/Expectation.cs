namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using CodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

/// A lightweight result to be used with <see cref="Result{S,F}"/>
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class Expectation
{
	public abstract string Message { get; }

	[CodeAnalysis.ExcludeFromCodeCoverage] public sealed override string ToString() => $"{Id( this )}: \"{Message}\"";
}

public sealed class CustomExpectation : Expectation
{
	[CodeAnalysis.ExcludeFromCodeCoverage] public override string Message { get; }

	public CustomExpectation( string message )
	{
		Message = message;
	}
}
