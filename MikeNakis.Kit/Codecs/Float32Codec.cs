namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="float" />.
public sealed class Float32Codec : AbstractCodec<float>
{
	public static readonly Float32Codec Instance = new();

	Float32Codec() { }

	public override void WriteText( float value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[10];
		bool ok = value.TryFormat( destination, out int charsWritten, "g6", provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		Sys.Span<char> result = destination[..charsWritten];
		Assert( FloatExactlyEquals( this.ReadText( result, mode ), value ) );
		textConsumer.Invoke( result );
	}

	public override Result<float, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !float.TryParse( charSpan, SysGlob.NumberStyles.AllowDecimalPoint | SysGlob.NumberStyles.AllowExponent | SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out float value ) )
			return Result<float, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a 32-bit floating-point number." ) );
		return Result<float, Expectation>.Success( value );
	}

	public override void WriteBinary( float value, BinaryStreamWriter binaryStreamWriter )
	{
		Assert( false ); //the following code is not necessarily correct
		Sys.Span<byte> bytes = stackalloc byte[4];
		bool ok = Sys.BitConverter.TryWriteBytes( bytes, value );
		Assert( ok );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		binaryStreamWriter.WriteBytes( bytes );
	}

	public override float ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		Assert( false ); //the following code is not necessarily correct
		Sys.Span<byte> bytes = stackalloc byte[4];
		binaryStreamReader.ReadBytes( bytes );
		if( Sys.BitConverter.IsLittleEndian )
			bytes.Reverse();
		return Sys.BitConverter.ToSingle( bytes );
	}
}
