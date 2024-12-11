namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using Sys = System;

/// A <see cref="Codec{T}" /> for <see cref="bool" />.
public sealed class BoolCodec : AbstractCodec<bool>
{
	static bool fromByte( byte value )
	{
		return value switch
		{
			0 => false,
			1 => true,
			_ => throw new Sys.FormatException( $"Expected 0 or 1, got {value}" )
		};
	}

	static byte toByte( bool value ) => value ? (byte)1 : (byte)0;

	public static readonly BoolCodec Instance = new();

	BoolCodec() { }

	public override void WriteText( bool value, TextConsumer textConsumer, Codec.Mode mode )
	{
		textConsumer.Invoke( value ? "true" : "false" );
	}

	public override Result<bool, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( charSpan.Equals2( "true" ) )
			return Result<bool, Expectation>.Success( true );
		if( charSpan.Equals2( "false" ) )
			return Result<bool, Expectation>.Success( false );
		return Result<bool, Expectation>.Failure( new CustomExpectation( $"expected 'true' or 'false', found '{charSpan}'" ) );
	}

	public override void WriteBinary( bool value, BinaryStreamWriter binaryStreamWriter ) => UInt8Codec.Instance.WriteBinary( toByte( value ), binaryStreamWriter );
	public override bool ReadBinary( BinaryStreamReader binaryStreamReader ) => fromByte( UInt8Codec.Instance.ReadBinary( binaryStreamReader ) );
}
