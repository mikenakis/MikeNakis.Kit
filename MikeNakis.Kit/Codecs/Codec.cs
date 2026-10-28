namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit.IO;
using Sys = System;

///Provides conversions to/from string, to/from binary, etc. for instances of a particular <see cref="Sys.Type" />.
///TODO: research this: System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter( typeof( System.IO.FileAttributes ) );
public interface Codec
{
	enum Mode
	{
		Verbatim, //Strings are rendered and parsed as they are.
		Script //Strings are enclosed in quotes and escaped.
	}

	void WriteText( object? value, TextConsumer textConsumer, Mode mode );
	Result<object?> TryReadText( Sys.ReadOnlySpan<char> charSpan, Mode mode );
	void WriteBinary( object? value, BinaryStreamWriter binaryStreamWriter );
	object? ReadBinary( BinaryStreamReader binaryStreamReader );
}

///Provides conversions to/from string, to/from binary, etc. for instances of a particular <see cref="Sys.Type" />.
public interface Codec<T> : Codec
{
	void WriteText( T value, TextConsumer textConsumer, Mode mode );
	new Result<T> TryReadText( Sys.ReadOnlySpan<char> charSpan, Mode mode );
	void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter );
	new T ReadBinary( BinaryStreamReader binaryStreamReader );
}
