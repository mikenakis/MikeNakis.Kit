namespace MikeNakis.Kit;

using Sys = System;
using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public readonly record struct Either<S, F>
{
	public static implicit operator Either<S, F>( S success ) => Either<S, F>.Success( success );
	public static implicit operator Either<S, F>( F failure ) => Either<S, F>.Failure( failure );

	public static Either<S, F> Success( S success ) => new( true, success, default! );
	public static Either<S, F> Failure( F failure ) => new( false, default!, failure );

	public static Either<S, F> Convert<S2, F2>( Either<S2, F2> source ) where S2 : S where F2 : F
	{
		if( source.IsSuccess )
			return Success( source.Payload );
		return Failure( source.Expectation );
	}

	public bool IsSuccess { get; }
	readonly S success;
	readonly F failure;

	public S Payload
	{
		get
		{
			Assert( IsSuccess );
			return success;
		}
	}

	public F Expectation
	{
		get
		{
			Assert( !IsSuccess );
			return failure;
		}
	}

	Either( bool isSuccess, S success, F failure )
	{
		IsSuccess = isSuccess;
		this.success = success;
		this.failure = failure;
	}

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string ToString() => IsSuccess ? $"Success: {success}" : $"Failure: {failure}";

	public S OrThrow()
	{
		if( !IsSuccess )
			throw new AssertionFailureException();
		return Payload;
	}

	public S OrThrow( Sys.Func<F, Sys.Exception> exceptionFactory )
	{
		if( !IsSuccess )
			throw exceptionFactory.Invoke( Expectation );
		return Payload;
	}
}
