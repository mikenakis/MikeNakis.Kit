namespace MikeNakis.Kit.Codecs;

using System.Linq;
using MikeNakis.Kit.IO;
using static System.MemoryExtensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;
using SysInterop = System.Runtime.InteropServices;

/// A <see cref="Codec{T}" /> for <see cref="decimal" />.
public sealed class DecimalCodec : AbstractCodec<decimal> //TODO: rename to Fixed128
{
	public static readonly DecimalCodec Instance = new();

	DecimalCodec() { }

	public override void WriteText( decimal value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[20];
		bool ok = value.TryFormat( destination, out int charsWritten, "G", provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Write( destination[..charsWritten] );
	}

	public override Result<decimal> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !decimal.TryParse( charSpan, SysGlob.NumberStyles.AllowDecimalPoint | SysGlob.NumberStyles.AllowExponent | SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out decimal value ) )
			return new CustomError( $"could not parse '{charSpan}' as a 128-bit fixed-point number." );
		return value;
	}

	public override void WriteBinary( decimal value, BinaryStreamWriter binaryStreamWriter )
	{
		Sys.Span<byte> bytes = stackalloc byte[16];
		decimal.GetBits( value, SysInterop.MemoryMarshal.Cast<byte, int>( bytes ) );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override decimal ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Sys.Span<byte> bytes = stackalloc byte[16];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return new decimal( SysInterop.MemoryMarshal.Cast<byte, int>( bytes ) );
	}
}
