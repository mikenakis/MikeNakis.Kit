namespace MikeNakis.Kit.Codecs;

using System.Linq;
using MikeNakis.Kit.IO;
using static System.MemoryExtensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="int" />.
public sealed class Int32Codec : AbstractCodec<int>
{
	public static readonly Int32Codec Instance = new();

	Int32Codec() { }

	public override void WriteText( int value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[10];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Write( destination[..charsWritten] );
	}

	public override Result<int> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !int.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out int value ) )
			return new CustomError( $"could not parse '{charSpan}' as a signed 32-bit integer number." );
		return value;
	}

	public override void WriteBinary( int value, BinaryStreamWriter binaryStreamWriter )
	{
		Sys.Span<byte> bytes = stackalloc byte[4];
		bool ok = Sys.BitConverter.TryWriteBytes( bytes, value );
		Assert( ok );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override int ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Sys.Span<byte> bytes = stackalloc byte[4];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return Sys.BitConverter.ToInt32( bytes );
	}
}
