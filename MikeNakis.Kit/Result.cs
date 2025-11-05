namespace MikeNakis.Kit;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

/// <summary>Represents the outcome of an operation as either success or failure; contains an expectation in the case of
/// failure.</summary>
public readonly struct Result : Sys.IEquatable<Result>
{
	static readonly Result successInstance = new( null );
	public static Result Success() => successInstance;

	public static implicit operator Result( Expectation expectation ) => new( expectation );

	readonly Expectation? expectation;

	Result( Expectation? expectation )
	{
		this.expectation = expectation;
	}

	public bool IsSuccess => expectation == null;
	public bool IsFailure => expectation != null;
	public bool Equals( Result other ) => Equals( expectation, other.expectation );
	public override int GetHashCode() => expectation == null ? 0 : expectation.GetHashCode();
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result kin && Equals( kin );

	public Expectation Expectation => expectation.OrThrow();

	public void OrThrow() => OrThrow( expectation => new ExpectationFailureException( expectation ) );

	public void OrThrow( Sys.Func<Expectation, Sys.Exception> converter )
	{
		if( expectation == null )
			return;
		Sys.Exception exception = converter.Invoke( expectation );
		throw exception;
	}

	/// <summary>Creates a successful <see cref="Result{S}"/> with a given payload.</summary>
	/// <remarks>Although the <see cref="Result{S}"/> class has implicit operators for casting either a payload or an
	/// expectation to an instance of <see cref="Result{S}"/>, in some cases this does not work, (for example, when the
	/// payload is of type <see cref="IReadOnlyList{T}"/>,) so this static factory method must be used instead.</remarks>
	public static Result<S> Success<S>( S payload ) => Result<S>.Success( payload );
}

/// <summary>Represents the outcome of an operation as either success or failure; contains a payload in the case of
/// success, or an expectation in the case of failure.</summary>
public readonly struct Result<S> : Sys.IEquatable<Result<S>>
{
	public static implicit operator Result<S>( Expectation expectation ) => Failure( expectation );
	public static implicit operator Result<S>( S payload ) => Success( payload );
	internal static Result<S> Success( S payload ) => new( true, payload, null );
	internal static Result<S> Failure( Expectation expectation ) => new( false, default!, expectation );
	public static Result<S> Convert<S2>( Result<S2> source ) where S2 : S
	{
		if( source.IsSuccess )
			return Success( source.Payload );
		return Failure( source.Expectation );
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	readonly S payload;
	readonly Expectation? expectation;

	Result( bool success, S payload, Expectation? expectation )
	{
		Assert( !typeof( Expectation ).IsAssignableFrom( typeof( S ) ) ); //You cannot have Result<Expectation> because then I would not be able to tell whether it is a success or a failure.
		IsSuccess = success;
		this.payload = payload;
		this.expectation = expectation;
	}

	public bool Equals( Result<S> other ) => IsSuccess ? DotNetHelpers.Equals( Payload, other.Payload ) : DotNetHelpers.Equals( expectation, other.expectation );
	public override int GetHashCode() => Sys.HashCode.Combine( IsSuccess, IsSuccess ? Payload?.GetHashCode() ?? 0 : Expectation.GetHashCode() );
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result<S> kin && Equals( kin );
	public Result<T> Convert<T>( Sys.Func<S, T> payloadConverter ) where T : notnull //
		=> IsFailure ? Result<T>.Failure( Expectation ) : Result<T>.Success( payloadConverter.Invoke( Payload ) );
	public Result<T> Convert<T>( Sys.Func<S, T> payloadConverter, Sys.Func<Expectation, Expectation> exceptionConverter ) where T : notnull //
		=> IsFailure ? Result<T>.Failure( exceptionConverter.Invoke( Expectation ) ) : Result<T>.Success( payloadConverter.Invoke( Payload ) );

	public S Payload
	{
		get
		{
			Assert( IsSuccess );
			return payload;
		}
	}

	public Expectation Expectation
	{
		get
		{
			Assert( IsFailure );
			return expectation!;
		}
	}

	public S OrThrow() => OrThrow( expectation => new ExpectationFailureException( expectation ) );

	public S OrThrow( Sys.Func<Expectation, Sys.Exception> converter )
	{
		if( IsSuccess )
			return Payload;
		Sys.Exception exception = converter.Invoke( expectation! );
		throw exception;
	}
}
