namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="byte" />.
public sealed class UInt8Codec : AbstractCodec<byte>
{
	public static readonly UInt8Codec Instance = new();

	UInt8Codec() { }

	public override void WriteText( byte value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[4];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<byte, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !byte.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out byte result ) )
			return Result<byte, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as an unsigned 8-bit integer number." ) );
		return Result<byte, Expectation>.Success( result );
	}

	public override void WriteBinary( byte value, BinaryStreamWriter binaryStreamWriter ) => Int8Codec.Instance.WriteBinary( (sbyte)value, binaryStreamWriter );
	public override byte ReadBinary( BinaryStreamReader binaryStreamReader ) => (byte)Int8Codec.Instance.ReadBinary( binaryStreamReader );
}
