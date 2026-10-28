namespace MikeNakis.Kit;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

/// <summary>Represents the outcome of an operation as either success or failure; contains an 
/// <see cref="Error"/> in the case of failure.</summary>
public readonly struct Result : Sys.IEquatable<Result>
{
	static readonly Result successInstance = new( null );
	public static Result Success() => successInstance;

	public static implicit operator Result( Error error ) => new( error );

	readonly Error? error;

	Result( Error? error )
	{
		this.error = error;
	}

	public bool IsSuccess => error == null;
	public bool IsFailure => error != null;
	public bool Equals( Result other ) => Equals( error, other.error );
	public override int GetHashCode() => error == null ? 0 : error.GetHashCode();
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result kin && Equals( kin );

	public Error Error => error.OrThrow();

	/// <summary>Throws an exception, if this is a failure.</summary>
	public void OrThrow() => OrThrow( error => new ErrorException( error ) );

	/// <summary>Throws an exception, if this is a failure.</summary>
	public void OrThrow( Sys.Func<Error, Sys.Exception> converter )
	{
		if( error == null )
			return;
		Sys.Exception exception = converter.Invoke( error );
		throw exception;
	}

	/// <summary>Creates a successful <see cref="Result{S}"/> with a given payload.</summary>
	/// <remarks>Although the <see cref="Result{S}"/> class has implicit operators for casting either a payload or an
	/// <see cref="Error"/> to an instance of <see cref="Result{S}"/>, in some cases this does not work, (for example, when the
	/// payload is of type <see cref="IReadOnlyList{T}"/>,) so this static factory method must be used instead.</remarks>
	public static Result<S> Success<S>( S payload ) => Result<S>.Success( payload );
}

/// <summary>Represents the outcome of an operation as either success or failure; contains a payload in the case of
/// success, or an <see cref="Error"/> in the case of failure.</summary>
public readonly struct Result<S> : Sys.IEquatable<Result<S>>
{
	public static implicit operator Result<S>( Error error ) => Failure( error );
	public static implicit operator Result<S>( S payload ) => Success( payload );
	internal static Result<S> Success( S payload ) => new( true, payload, null );
	internal static Result<S> Failure( Error error ) => new( false, default!, error );
	public static Result<S> Convert<S2>( Result<S2> source ) where S2 : S
	{
		if( source.IsSuccess )
			return Success( source.Payload );
		return Failure( source.Error );
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	readonly S payload;
	readonly Error? error;

	Result( bool success, S payload, Error? error )
	{
		Assert( !typeof( Error ).IsAssignableFrom( typeof( S ) ) ); //You cannot have Result<Error> because then I would not be able to tell whether it is a success or a failure.
		IsSuccess = success;
		this.payload = payload;
		this.error = error;
	}

	public bool Equals( Result<S> other ) => IsSuccess ? DotNetHelpers.Equals( Payload, other.Payload ) : DotNetHelpers.Equals( error, other.error );
	public override int GetHashCode() => Sys.HashCode.Combine( IsSuccess, IsSuccess ? Payload?.GetHashCode() ?? 0 : Error.GetHashCode() );
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result<S> kin && Equals( kin );
	public Result<T> Convert<T>( Sys.Func<S, T> payloadConverter ) where T : notnull //
		=> IsFailure ? Result<T>.Failure( Error ) : Result<T>.Success( payloadConverter.Invoke( Payload ) );
	public Result<T> Convert<T>( Sys.Func<S, T> payloadConverter, Sys.Func<Error, Error> exceptionConverter ) where T : notnull //
		=> IsFailure ? Result<T>.Failure( exceptionConverter.Invoke( Error ) ) : Result<T>.Success( payloadConverter.Invoke( Payload ) );

	public S Payload
	{
		get
		{
			Assert( IsSuccess );
			return payload;
		}
	}

	public Error Error
	{
		get
		{
			Assert( IsFailure );
			return error!;
		}
	}

	public S OrThrow() => OrThrow( error => new ErrorException( error ) );

	public S OrThrow( Sys.Func<Error, Sys.Exception> converter )
	{
		if( IsSuccess )
			return Payload;
		Sys.Exception exception = converter.Invoke( error! );
		throw exception;
	}
}
