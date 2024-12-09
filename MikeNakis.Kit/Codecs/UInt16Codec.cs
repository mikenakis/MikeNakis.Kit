namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="ushort" />.
public sealed class UInt16Codec : AbstractCodec<ushort>
{
	public static readonly UInt16Codec Instance = new();

	UInt16Codec() { }

	public override void WriteText( ushort value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[7];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<ushort, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !ushort.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out ushort value ) )
			return Result<ushort, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as an unsigned 16-bit integer number." ) );
		return Result<ushort, Expectation>.Success( value );
	}

	public override void WriteBinary( ushort value, BinaryStreamWriter binaryStreamWriter ) => Int16Codec.Instance.WriteBinary( (short)value, binaryStreamWriter );
	public override ushort ReadBinary( BinaryStreamReader binaryStreamReader ) => (ushort)Int16Codec.Instance.ReadBinary( binaryStreamReader );
}
