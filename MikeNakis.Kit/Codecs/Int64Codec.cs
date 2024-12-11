namespace MikeNakis.Kit.Codecs;

using System.Linq;
using MikeNakis.Kit;
using static System.MemoryExtensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="long" />
public sealed class Int64Codec : AbstractCodec<long>
{
	public static readonly Int64Codec Instance = new();

	Int64Codec() { }

	public override void WriteText( long value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[20];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<long, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !long.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out long value ) )
			return Result<long, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a signed 64-bit integer number." ) );
		return Result<long, Expectation>.Success( value );
	}

	public override void WriteBinary( long value, BinaryStreamWriter binaryStreamWriter )
	{
		Sys.Span<byte> bytes = stackalloc byte[8];
		bool ok = Sys.BitConverter.TryWriteBytes( bytes, value );
		Assert( ok );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override long ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Sys.Span<byte> bytes = stackalloc byte[8];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return Sys.BitConverter.ToInt64( bytes );
	}

	static long longFromString() //TODO why is this not marked as unused?
	{
		return 0L;
	}
}
