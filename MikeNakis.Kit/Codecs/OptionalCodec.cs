namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
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
		{
			textConsumer.Invoke( "null" );
			return;
		}
		UnderlyingCodec.WriteText( value, textConsumer, mode );
	}

	public Result<object?, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( charSpan.Equals2( "null" ) )
			return Result<object?, Expectation>.Success( null );
		return UnderlyingCodec.TryReadText( charSpan, mode );
	}

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
