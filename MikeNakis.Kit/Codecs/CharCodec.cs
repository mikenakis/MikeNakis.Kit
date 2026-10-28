namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
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
				textConsumer.Write( new Sys.ReadOnlySpan<char>( in value ) );
				break;
			case Codec.Mode.Script:
				KitHelpers.ScribeStringLiteral( '\'', new string( value, 1 ), textConsumer );
				break;
			default:
				throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null );
		}
	}

	public override Result<char> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		return mode switch
		{
			Codec.Mode.Verbatim => charSpan.Length == 1 ? charSpan[0] : new CustomError( $"expected a single character, found '{charSpan}'" ),
			Codec.Mode.Script => tryFromScript( charSpan ),
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null )
		};

		static Result<char> tryFromScript( Sys.ReadOnlySpan<char> charSpan )
		{
			Result<string> result = KitHelpers.ParseStringLiteral( '\'', charSpan );
			if( !result.IsSuccess )
				return result.Error;
			string s = result.Payload;
			if( s.Length != 1 )
				return new CustomError( $"expected one character, found '{charSpan}'" );
			return s[0];
		}
	}

	public override void WriteBinary( char value, BinaryStreamWriter binaryStreamWriter ) => UInt16Codec.Instance.WriteBinary( value, binaryStreamWriter );
	public override char ReadBinary( BinaryStreamReader binaryStreamReader ) => (char)UInt16Codec.Instance.ReadBinary( binaryStreamReader );
}
