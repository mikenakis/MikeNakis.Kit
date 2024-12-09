namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
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
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<int, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !int.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out int value ) )
			return Result<int, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a signed 32-bit integer number." ) );
		return Result<int, Expectation>.Success( value );
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

	static int integerFromString( string content ) //FIXME XXX TODO why is this method not marked as unused?
	{
		if( !int.TryParse( content, SysGlob.NumberStyles.Integer, SysGlob.NumberFormatInfo.InvariantInfo, out int result ) )
			throw KitHelpers.NewFormatException( nameof( Sys.Int32 ), content );
		return result;
	}
}
