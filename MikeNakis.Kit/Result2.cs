namespace MikeNakis.Kit;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

/// <summary>Represents the outcome of an operation as either success or failure; contains an exception in the
/// case of failure.</summary>
public readonly struct Result2 : Sys.IEquatable<Result2>
{
	static readonly Result2 successInstance = new( null );
	public static Result2 Success() => successInstance;

	public static implicit operator Result2( Sys.Exception exception ) => new( exception );

	readonly Sys.Exception? exception;

	Result2( Sys.Exception? exception )
	{
		this.exception = exception;
	}

	public bool IsSuccess => exception == null;
	public bool IsFailure => exception != null;
	public bool Equals( Result2 other ) => Equals( exception, other.exception );
	public override int GetHashCode() => exception == null ? 0 : exception.GetHashCode();
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result2 kin && Equals( kin );

	public Sys.Exception Exception => exception.OrThrow();

	/// <summary>Throws an exception, if this is a failure.</summary>
	public void OrThrow() => OrThrow( exception => new ResultFailureException( exception ) );

	/// <summary>Throws an exception, if this is a failure.</summary>
	public void OrThrow( Sys.Func<Sys.Exception, Sys.Exception> converter )
	{
		if( exception == null )
			return;
		Sys.Exception convertedException = converter.Invoke( exception );
		throw convertedException;
	}

	/// <summary>Creates a successful <see cref="Result2{S}"/> with a given payload.</summary>
	/// <remarks>Although the <see cref="Result2{S}"/> class has implicit operators for casting either a payload
	/// or an exception to an instance of <see cref="Result2{S}"/>, in some cases this does not work, (for example, when
	/// the payload is of type <see cref="IReadOnlyList{T}"/>,) so this static factory method must be used instead.</remarks>
	public static Result2<S> Success<S>( S payload ) => Result2<S>.Success( payload );
}

/// <summary>Represents the outcome of an operation as either success or failure; contains a payload in the
/// case of success, or an exception in the case of failure.</summary>
public readonly struct Result2<S> : Sys.IEquatable<Result2<S>>
{
	public static implicit operator Result2<S>( Sys.Exception exception ) => Failure( exception );
	public static implicit operator Result2<S>( S payload ) => Success( payload );
	internal static Result2<S> Success( S payload ) => new( true, payload, null );
	internal static Result2<S> Failure( Sys.Exception exception ) => new( false, default!, exception );
	public static Result2<S> Convert<S2>( Result2<S2> source ) where S2 : S
	{
		if( source.IsSuccess )
			return Success( source.Payload );
		return Failure( source.Exception );
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	readonly S payload;
	readonly Sys.Exception? exception;

	Result2( bool success, S payload, Sys.Exception? exception )
	{
		Assert( !typeof( Sys.Exception ).IsAssignableFrom( typeof( S ) ) ); //You cannot have Result2<Exception> because then I would not be able to tell whether it is a success or a failure.
		IsSuccess = success;
		this.payload = payload;
		this.exception = exception;
	}

	public bool Equals( Result2<S> other ) => IsSuccess ? DotNetHelpers.Equals( Payload, other.Payload ) : DotNetHelpers.Equals( exception, other.exception );
	public override int GetHashCode() => Sys.HashCode.Combine( IsSuccess, IsSuccess ? Payload?.GetHashCode() ?? 0 : Exception.GetHashCode() );
	[Sys.Obsolete] public override bool Equals( object? obj ) => obj is Result2<S> kin && Equals( kin );
	public Result2<T> Convert<T>( Sys.Func<S, T> payloadConverter ) where T : notnull //
		=> IsFailure ? Result2<T>.Failure( Exception ) : Result2<T>.Success( payloadConverter.Invoke( Payload ) );
	public Result2<T> Convert<T>( Sys.Func<S, T> payloadConverter, Sys.Func<Sys.Exception, Sys.Exception> exceptionConverter ) where T : notnull //
		=> IsFailure ? Result2<T>.Failure( exceptionConverter.Invoke( Exception ) ) : Result2<T>.Success( payloadConverter.Invoke( Payload ) );

	public S Payload
	{
		get
		{
			Assert( IsSuccess );
			return payload;
		}
	}

	public Sys.Exception Exception
	{
		get
		{
			Assert( IsFailure );
			return exception!;
		}
	}

	/// <summary>Throws an exception, if this is a failure.</summary>
	public S OrThrow() => OrThrow( exception => new ResultFailureException( exception ) );

	/// <summary>Throws an exception, if this is a failure.</summary>
	public S OrThrow( Sys.Func<Sys.Exception, Sys.Exception> converter )
	{
		if( IsSuccess )
			return Payload;
		Sys.Exception convertedException = converter.Invoke( exception! );
		throw convertedException;
	}
}
