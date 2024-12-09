namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using MikeNakis.Kit.Collections;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysDiag = System.Diagnostics;

/// A <see cref="Codec{T}" /> for <see cref="Sys.Enum" />.
// PEARL: "class C<T> where T : Sys.Enum" is not enough; it must be "class C<T> where T : struct, Sys.Enum" instead.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class CSharpEnumCodec<T> : AbstractCodec<T> where T : struct, Sys.Enum
{
	readonly IReadOnlyDictionary<T, string> valuesToNames;
	readonly IReadOnlyDictionary<string, T> namesToValues;
	readonly Sys.Action<T, BinaryStreamWriter> binaryWriter;
	readonly Sys.Func<BinaryStreamReader, T> binaryReader;

	public CSharpEnumCodec()
	{
		Sys.Type underlyingType = typeof( T ).GetEnumUnderlyingType();
		valuesToNames = Sys.Enum.GetValues<T>().ToDictionary( v => v, v => Sys.Enum.GetName( v )! );
		namesToValues = OrderedDictionary.InverseOf( valuesToNames );
		binaryReader = underlyingType == typeof( int ) ? intBinaryReader : throw new Sys.NotImplementedException();
		binaryWriter = underlyingType == typeof( int ) ? intBinaryWriter : throw new Sys.NotImplementedException();
	}

	public override void WriteText( T value, TextConsumer textConsumer, Codec.Mode mode )
	{
		textConsumer.Invoke( valuesToNames[value] );
	}

	public override Result<T, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		if( !namesToValues.TryGetValue( charSpan.ToString(), out T value ) )
			return Result<T, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a {nameof( T )}." ) );
		return Result<T, Expectation>.Success( value );
	}

	public override void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter ) => binaryWriter.Invoke( value, binaryStreamWriter );
	public override T ReadBinary( BinaryStreamReader binaryStreamReader ) => binaryReader.Invoke( binaryStreamReader );
	public override string ToString() => $"{Id( this )} {nameof( T )}";

	static void intBinaryWriter( T value, BinaryStreamWriter binaryStreamWriter ) => Int32Codec.Instance.WriteBinary( (int)(object)value, binaryStreamWriter );
	static T intBinaryReader( BinaryStreamReader binaryStreamReader ) => (T)Sys.Enum.ToObject( typeof( int ), Int32Codec.Instance.ReadBinary( binaryStreamReader ) );
}
