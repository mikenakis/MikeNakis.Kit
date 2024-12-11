namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using Sys = System;

/// A <see cref="Codec{T}" /> for <see cref="char" />.
public sealed class CharCodec : AbstractCodec<char>
{
	public static readonly CharCodec Instance = new();

	CharCodec() { }

	public override void WriteText( char value, TextConsumer textConsumer, Codec.Mode mode )
	{
		switch( mode )
		{
			case Codec.Mode.Verbatim:
				textConsumer.Invoke( new Sys.ReadOnlySpan<char>( in value ) );
				break;
			case Codec.Mode.Script:
				KitHelpers.ScribeStringLiteral( '\'', new Sys.ReadOnlySpan<char>( in value ), textConsumer );
				break;
			default:
				throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null );
		}
	}

	public override Result<char, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		return mode switch
		{
			Codec.Mode.Verbatim => charSpan.Length == 1 ? Result<char, Expectation>.Success( charSpan[0] ) : Result<char, Expectation>.Failure( new CustomExpectation( $"expected a single character, found '{charSpan}'" ) ),
			Codec.Mode.Script => tryFromScript( charSpan ),
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null )
		};

		static Result<char, Expectation> tryFromScript( Sys.ReadOnlySpan<char> charSpan )
		{
			Result<string, Expectation> result = StringCodec.ParseStringLiteral( '\'', charSpan );
			if( !result.IsSuccess )
				return Result<char, Expectation>.Failure( result.AsFailure );
			string s = result.AsSuccess;
			if( s.Length != 1 )
				return Result<char, Expectation>.Failure( new CustomExpectation( $"expected one character, found '{charSpan}'" ) );
			return Result<char, Expectation>.Success( s[0] );
		}
	}

	public override void WriteBinary( char value, BinaryStreamWriter binaryStreamWriter )
	{
		UInt16Codec.Instance.WriteBinary( value, binaryStreamWriter );
	}

	public override char ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		return (char)UInt16Codec.Instance.ReadBinary( binaryStreamReader );
	}
}
