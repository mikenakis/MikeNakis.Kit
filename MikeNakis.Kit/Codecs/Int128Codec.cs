namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="Sys.Int128" />
public sealed class Int128Codec : AbstractCodec<Sys.Int128>
{
	public static readonly Int128Codec Instance = new();

	Int128Codec() { }

	public override void WriteText( Sys.Int128 value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[16];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<Sys.Int128, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !Sys.Int128.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out Sys.Int128 value ) )
			return Result<Sys.Int128, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a signed 128-bit integer number." ) );
		return Result<Sys.Int128, Expectation>.Success( value );
	}

	public override void WriteBinary( Sys.Int128 value, BinaryStreamWriter binaryStreamWriter )
	{
		throw new Sys.NotImplementedException(); // waiting for BitConverter.GetBytes( Sys.Int128 ) in dotnet 9 (see https://github.com/dotnet/runtime/issues/80337)
	}

	public override Sys.Int128 ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		throw new Sys.NotImplementedException(); // waiting for BitConverter.ToInt128( span ) in dotnet 9 (see https://github.com/dotnet/runtime/issues/80337)
	}

	static Sys.Int128 int128FromString( string content ) //TODO why is this not marked as unused?
	{
		const SysGlob.NumberStyles options = SysGlob.NumberStyles.Integer;
		if( !Sys.Int128.TryParse( content, options, SysGlob.NumberFormatInfo.InvariantInfo, out Sys.Int128 result ) )
			throw KitHelpers.NewFormatException( nameof( Sys.Int128 ), content );
		return result;
	}
}
