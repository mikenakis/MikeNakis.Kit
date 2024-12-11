namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="sbyte" />.
public sealed class Int8Codec : AbstractCodec<sbyte>
{
	public static readonly Int8Codec Instance = new();

	Int8Codec() { }

	public override void WriteText( sbyte value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[5];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<sbyte, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !sbyte.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out sbyte value ) )
			return Result<sbyte, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a signed 8-bit integer number." ) );
		return Result<sbyte, Expectation>.Success( value );
	}

	public override void WriteBinary( sbyte value, BinaryStreamWriter binaryStreamWriter )
	{
		Sys.Span<byte> bytes = stackalloc byte[1];
		bytes[0] = (byte)value;
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override sbyte ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Sys.Span<byte> bytes = stackalloc byte[1];
		binaryStreamReader.ReadBytes( bytes );
		return (sbyte)bytes[0];
	}
}
