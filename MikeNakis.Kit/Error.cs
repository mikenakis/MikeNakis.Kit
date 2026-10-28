namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

/// A lightweight class to be used instead of an exception.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class Error
{
	public abstract string Message { get; }

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public sealed override string ToString() => $"{Id( this )}: \"{Message}\"";

	public ErrorException Throw()
	{
		throw new ErrorException( this );
	}
}

public sealed class CustomError : Error
{
	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string Message { get; }

	public CustomError( string message )
	{
		Message = message;
	}
}

/// [Claude] An <see cref="Error"/> that carries an exception, for failures that were thrown rather than anticipated.
public sealed class ExceptionError : Error
{
	public Sys.Exception Exception { get; }
	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string Message => $"{Exception.GetType().Name}: {Exception.Message}";

	public ExceptionError( Sys.Exception exception )
	{
		Exception = exception;
	}
}
