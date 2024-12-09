namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="short" />.
public sealed class Int16Codec : AbstractCodec<short>
{
	public static readonly Int16Codec Instance = new();

	Int16Codec() { }

	public override void WriteText( short value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[7];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<short, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !short.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out short value ) )
			return Result<short, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a signed 16-bit integer number." ) );
		return Result<short, Expectation>.Success( value );
	}

	public override void WriteBinary( short value, BinaryStreamWriter binaryStreamWriter )
	{
		Sys.Span<byte> bytes = stackalloc byte[2];
		bool ok = Sys.BitConverter.TryWriteBytes( bytes, value );
		Assert( ok );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override short ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Sys.Span<byte> bytes = stackalloc byte[2];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return Sys.BitConverter.ToInt16( bytes );
	}
}
