namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="uint" />.
public sealed class UInt32Codec : AbstractCodec<uint>
{
	public static readonly UInt32Codec Instance = new();

	UInt32Codec() { }

	public override void WriteText( uint value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[10];
		bool ok = value.TryFormat( destination, out int charsWritten, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Write( destination[..charsWritten] );
	}

	public override Result<uint> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !uint.TryParse( charSpan, SysGlob.NumberStyles.AllowLeadingSign, SysGlob.CultureInfo.InvariantCulture, out uint value ) )
			return new CustomError( $"could not parse '{charSpan}' as a signed 32-bit integer number." );
		return value;
	}

	public override void WriteBinary( uint value, BinaryStreamWriter binaryStreamWriter ) => Int32Codec.Instance.WriteBinary( (int)value, binaryStreamWriter );
	public override uint ReadBinary( BinaryStreamReader binaryStreamReader ) => (uint)Int32Codec.Instance.ReadBinary( binaryStreamReader );

	static int integerFromString( string content ) //FIXME XXX TODO why is this method not marked as unused?
	{
		if( !int.TryParse( content, SysGlob.NumberStyles.Integer, SysGlob.NumberFormatInfo.InvariantInfo, out int result ) )
			throw KitHelpers.NewFormatException( typeof( uint ).Name, content );
		return result;
	}
}
