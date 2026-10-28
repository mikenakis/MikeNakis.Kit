namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="ulong" />
public sealed class UInt64Codec : AbstractCodec<ulong>
{
	public static readonly UInt64Codec Instance = new();

	UInt64Codec() { }

	public override void WriteText( ulong value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[20];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Write( destination[..charsWritten] );
	}

	public override Result<ulong> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !ulong.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out ulong value ) )
			return new CustomError( $"could not parse '{charSpan}' as an unsigned 64-bit integer number." );
		return value;
	}

	public override void WriteBinary( ulong value, BinaryStreamWriter binaryStreamWriter ) => Int64Codec.Instance.WriteBinary( (long)value, binaryStreamWriter );
	public override ulong ReadBinary( BinaryStreamReader binaryStreamReader ) => (ulong)Int64Codec.Instance.ReadBinary( binaryStreamReader );
}
