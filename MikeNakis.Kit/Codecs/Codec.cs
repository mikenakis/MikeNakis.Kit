namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;

///Provides conversions to/from string, to/from binary, etc. for instances of a particular <see cref="Sys.Type" />.
public interface Codec
{
	public enum Mode
	{
		Verbatim, //Strings are rendered and parsed as they are.
		Script //Strings are enclosed in quotes and escaped.
	}

	void WriteText( object? value, TextConsumer textConsumer, Mode mode );
	Result<object?, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Mode mode );
	void WriteBinary( object? value, BinaryStreamWriter binaryStreamWriter );
	object? ReadBinary( BinaryStreamReader binaryStreamReader );
}

///Provides conversions to/from string, to/from binary, etc. for instances of a particular <see cref="Sys.Type" />.
public interface Codec<T> : Codec
{
	void WriteText( T value, TextConsumer textConsumer, Mode mode );
	new Result<T, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Mode mode );
	void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter );
	new T ReadBinary( BinaryStreamReader binaryStreamReader );
}
