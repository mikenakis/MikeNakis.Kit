namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysDiag = System.Diagnostics;

/// A <see cref="Codec{T}" /> for types that have an <see cref="int" /> representation.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class IntegerRepresentationCodec<T> : AbstractCodec<T> where T : notnull
{
	readonly Sys.Func<int, T> valueFromInteger;
	readonly Sys.Func<T, int> integerFromValue;

	public IntegerRepresentationCodec( Sys.Func<int, T> valueFromInteger, Sys.Func<T, int> integerFromValue )
	{
		this.valueFromInteger = valueFromInteger;
		this.integerFromValue = integerFromValue;
	}

	public override void WriteText( T value, TextConsumer textConsumer, Codec.Mode mode ) => Int32Codec.Instance.WriteText( integerFromValue( value ), textConsumer, mode );

	public override Result<T> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		Result<int> result = Int32Codec.Instance.TryReadText( charSpan, mode );
		if( !result.IsSuccess )
			return result.Error;
		T value = valueFromInteger( result.Payload );
		return value;
	}

	public override void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter ) => Int32Codec.Instance.WriteBinary( integerFromValue( value ), binaryStreamWriter );
	public override T ReadBinary( BinaryStreamReader binaryStreamReader ) => valueFromInteger( Int32Codec.Instance.ReadBinary( binaryStreamReader ) );
	public override string ToString() => $"{Id( this )} {typeof( T ).Name}";
}
