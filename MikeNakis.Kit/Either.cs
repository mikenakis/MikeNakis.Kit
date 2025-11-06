namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCodeAnalysis = System.Diagnostics.CodeAnalysis;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public readonly record struct Either<L, R>
{
	public static implicit operator Either<L, R>( L left ) => Either<L, R>.CreateWithLeft( left );
	public static implicit operator Either<L, R>( R right ) => Either<L, R>.CreateWithRight( right );

	public static Either<L, R> CreateWithLeft( L success ) => new( true, success, default! );
	public static Either<L, R> CreateWithRight( R failure ) => new( false, default!, failure );

	public static Either<L, R> Convert<S2, F2>( Either<S2, F2> source ) where S2 : L where F2 : R
	{
		if( source.IsLeft )
			return CreateWithLeft( source.Left );
		return CreateWithRight( source.Right );
	}

	public bool IsLeft { get; }
	readonly L left;
	readonly R right;

	public L Left
	{
		get
		{
			Assert( IsLeft );
			return left;
		}
	}

	public R Right
	{
		get
		{
			Assert( !IsLeft );
			return right;
		}
	}

	Either( bool isSuccess, L left, R right )
	{
		IsLeft = isSuccess;
		this.left = left;
		this.right = right;
	}

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string ToString() => IsLeft ? $"Success: {left}" : $"Failure: {right}";

	public L OrThrow()
	{
		if( !IsLeft )
			throw new AssertionFailureException();
		return Left;
	}

	public L OrThrow( Sys.Func<R, Sys.Exception> exceptionFactory )
	{
		if( !IsLeft )
			throw exceptionFactory.Invoke( Right );
		return Left;
	}
}
