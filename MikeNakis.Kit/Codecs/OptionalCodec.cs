namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.IO;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysDiag = System.Diagnostics;

///<summary>A <see cref="Codec"/> for values that may be <c>null</c>.</summary>
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class OptionalCodec : Codec
{
	public Codec UnderlyingCodec { get; }

	public OptionalCodec( Codec underlyingCodec )
	{
		Assert( underlyingCodec is not OptionalCodec );
		UnderlyingCodec = underlyingCodec;
	}

	public void WriteText( object? value, TextConsumer textConsumer, Codec.Mode mode )
	{
		if( value == null )
			textConsumer.Write( "null" );
		else
			UnderlyingCodec.WriteText( value, textConsumer, mode );
	}

	public Result<object?> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode ) => charSpan.Equals2( "null" ) ? Result.Success<object?>( null ) : UnderlyingCodec.TryReadText( charSpan, mode );

	public void WriteBinary( object? value, BinaryStreamWriter binaryStreamWriter )
	{
		BoolCodec.Instance.WriteBinary( value != null, binaryStreamWriter );
		if( value != null )
			UnderlyingCodec.WriteBinary( value, binaryStreamWriter );
	}

	public object? ReadBinary( BinaryStreamReader binaryStreamReader )
	{
		if( !BoolCodec.Instance.ReadBinary( binaryStreamReader ) )
			return default;
		return UnderlyingCodec.ReadBinary( binaryStreamReader );
	}

	public override string ToString() => $"{Id( this )} Underlying: {UnderlyingCodec}";
}
