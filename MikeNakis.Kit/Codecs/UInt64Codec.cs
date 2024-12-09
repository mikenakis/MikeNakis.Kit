namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
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
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<ulong, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !ulong.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out ulong value ) )
			return Result<ulong, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as an unsigned 64-bit integer number." ) );
		return Result<ulong, Expectation>.Success( value );
	}

	public override void WriteBinary( ulong value, BinaryStreamWriter binaryStreamWriter ) => Int64Codec.Instance.WriteBinary( (long)value, binaryStreamWriter );
	public override ulong ReadBinary( BinaryStreamReader binaryStreamReader ) => (ulong)Int64Codec.Instance.ReadBinary( binaryStreamReader );
}
