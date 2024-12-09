namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using CodeAnalysis = System.Diagnostics.CodeAnalysis;
using Sys = System;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public readonly record struct Result<S, F>
{
	public static Result<S, F> Success( S success ) => new( true, success, default! );
	public static Result<S, F> Failure( F failure ) => new( false, default!, failure );

	public static Result<S, F> Convert<S2, F2>( Result<S2, F2> result ) where S2 : S where F2 : F
	{
		if( result.IsSuccess )
			return Success( result.AsSuccess );
		return Failure( result.AsFailure );
	}

	public bool IsSuccess { get; }
	readonly S success;
	readonly F failure;

	public S AsSuccess
	{
		get
		{
			Assert( IsSuccess );
			return success;
		}
	}

	public F AsFailure
	{
		get
		{
			Assert( !IsSuccess );
			return failure;
		}
	}

	Result( bool isSuccess, S success, F failure )
	{
		IsSuccess = isSuccess;
		this.success = success;
		this.failure = failure;
	}

	[CodeAnalysis.ExcludeFromCodeCoverage] public override string ToString() => IsSuccess ? $"Success: {success}" : $"Failure: {failure}";

	public S OrThrow( Sys.Func<F, Sys.Exception> exceptionFactory )
	{
		if( !IsSuccess )
			throw exceptionFactory.Invoke( AsFailure );
		return AsSuccess;
	}
}
