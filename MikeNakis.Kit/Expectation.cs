namespace MikeNakis.Kit;

using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

/// A lightweight result to be used with <see cref="Either{S,F}"/> or, better yet, with <see cref="Result{S}"/>
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class Expectation
{
	public abstract string Message { get; }

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public sealed override string ToString() => $"{Id( this )}: \"{Message}\"";

	public ExpectationFailureException Exception()
	{
		throw new ExpectationFailureException( this );
	}
}

public sealed class CustomExpectation : Expectation
{
	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string Message { get; }

	public CustomExpectation( string message )
	{
		Message = message;
	}
}
