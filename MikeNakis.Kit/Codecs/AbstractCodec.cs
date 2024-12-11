namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using Sys = System;

/// Base class for implementations of <see cref="Codec{T}" />.
public abstract class AbstractCodec<T> : Codec<T>
{
	protected AbstractCodec()
	{ }

	void Codec.WriteText( object? value, TextConsumer textConsumer, Codec.Mode mode ) => WriteText( (T)value!, textConsumer, mode );
	Result<object?, Expectation> Codec.TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode ) => Result<object?, Expectation>.Convert( TryReadText( charSpan, mode ) );
	void Codec.WriteBinary( object? value, BinaryStreamWriter binaryStreamWriter ) => WriteBinary( (T)value!, binaryStreamWriter );
	object Codec.ReadBinary( BinaryStreamReader binaryStreamReader ) => ReadBinary( binaryStreamReader )!;

	public abstract void WriteText( T value, TextConsumer textConsumer, Codec.Mode mode );
	public abstract Result<T, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode );
	public abstract void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter );
	public abstract T ReadBinary( BinaryStreamReader binaryStreamReader );
}
