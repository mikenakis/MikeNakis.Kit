namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="Sys.UInt128" />
public sealed class UInt128Codec : AbstractCodec<Sys.UInt128>
{
	public static readonly UInt128Codec Instance = new();

	UInt128Codec() { }

	public override void WriteText( Sys.UInt128 value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[16];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<Sys.UInt128, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !Sys.UInt128.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out Sys.UInt128 value ) )
			return Result<Sys.UInt128, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as an unsigned 128-bit integer number." ) );
		return Result<Sys.UInt128, Expectation>.Success( value );
	}

	public override void WriteBinary( Sys.UInt128 value, BinaryStreamWriter binaryStreamWriter ) => Int128Codec.Instance.WriteBinary( (Sys.Int128)value, binaryStreamWriter );
	public override Sys.UInt128 ReadBinary( BinaryStreamReader binaryStreamReader ) => (Sys.UInt128)Int128Codec.Instance.ReadBinary( binaryStreamReader );
}
