namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="double" />.
public sealed class Float64Codec : AbstractCodec<double>
{
	public static readonly Float64Codec Instance = new();

	Float64Codec() { }

	public override void WriteText( double value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[16];
		bool ok = value.TryFormat( destination, out int charsWritten, "g15", provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<double, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !double.TryParse( charSpan, SysGlob.NumberStyles.AllowDecimalPoint | SysGlob.NumberStyles.AllowExponent | SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out double value ) )
			return Result<double, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a 64-bit floating-point number." ) );
		return Result<double, Expectation>.Success( value );
	}

	public override void WriteBinary( double value, BinaryStreamWriter binaryStreamWriter )
	{
		Assert( false ); //the following code is not necessarily correct
		Sys.Span<byte> bytes = stackalloc byte[8];
		bool ok = Sys.BitConverter.TryWriteBytes( bytes, value );
		Assert( ok );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override double ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Assert( false ); //the following code is not necessarily correct
		Sys.Span<byte> bytes = stackalloc byte[8];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return Sys.BitConverter.ToDouble( bytes );
	}
}
