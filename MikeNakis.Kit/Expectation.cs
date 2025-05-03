namespace MikeNakis.Kit;

/// A lightweight result to be used with <see cref="Result{S,F}"/>
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class Expectation
{
	public abstract string Message { get; }

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public sealed override string ToString() => $"{Id( this )}: \"{Message}\"";
}

public sealed class CustomExpectation : Expectation
{
	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string Message { get; }

	public CustomExpectation( string message )
	{
		Message = message;
	}
}
