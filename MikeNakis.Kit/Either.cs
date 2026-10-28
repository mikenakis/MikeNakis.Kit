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

	public static Either<L, R> CreateWithLeft( L left ) => new( true, left, default! );
	public static Either<L, R> CreateWithRight( R right ) => new( false, default!, right );

	public static Either<L, R> Convert<L2, R2>( Either<L2, R2> source ) where L2 : L where R2 : R
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

	Either( bool isLeft, L left, R right )
	{
		IsLeft = isLeft;
		this.left = left;
		this.right = right;
	}

	[SysCodeAnalysis.ExcludeFromCodeCoverage] public override string ToString() => IsLeft ? $"Left: {left}" : $"Right: {right}";

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
