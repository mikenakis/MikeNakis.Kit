namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysDiag = System.Diagnostics;

/// A <see cref="Codec{T}" /> for types that have a <see cref="string" /> representation.
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public sealed class StringRepresentationCodec<T> : AbstractCodec<T> where T : notnull
{
	readonly Sys.Func<string, T?> valueFromString;
	readonly Sys.Func<T, string> stringFromValue;

	public StringRepresentationCodec( Sys.Func<string, T> valueFromString, Sys.Func<T, string> stringFromValue )
	{
		this.valueFromString = valueFromString;
		this.stringFromValue = stringFromValue;
	}

	public override void WriteText( T value, TextConsumer textConsumer, Codec.Mode mode )
	{
		textConsumer.Invoke( stringFromValue( value ) );
	}

	public override Result<T, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		T? value = valueFromString( charSpan.ToString() );
		if( value is null )
			return Result<T, Expectation>.Failure( new CustomExpectation( $"could not parse {charSpan} as {nameof( T )}" ) );
		return Result<T, Expectation>.Success( value );
	}

	public override void WriteBinary( T value, BinaryStreamWriter binaryStreamWriter ) => StringCodec.Instance.WriteBinary( stringFromValue( value ), binaryStreamWriter );
	public override T ReadBinary( BinaryStreamReader binaryStreamReader ) => valueFromString( StringCodec.Instance.ReadBinary( binaryStreamReader ) ) ?? throw new Sys.FormatException();
	public override string ToString() => $"{Id( this )} {nameof( T )}";
}
