namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.KitHelpers;
using Sys = System;
using SysText = System.Text;

public static class CodecExtensions
{
	static Sys.Exception newFormatException( Expectation expectation ) => new Sys.FormatException( expectation.Message );

	public static string ToString( this Codec self, object? value ) //
		=> self.ToString( value, Codec.Mode.Verbatim );

	public static void WriteText( this Codec self, object? value, TextConsumer textConsumer ) //
		=> self.WriteText( value, textConsumer, Codec.Mode.Script );

	public static string ToString( this Codec self, object? value, Codec.Mode mode )
	{
		SysText.StringBuilder stringBuilder = new();
		TextConsumer textConsumer = s => stringBuilder.Append( s );
		self.WriteText( value, textConsumer, mode );
		return stringBuilder.ToString();
	}

	public static object? ReadText( this Codec self, Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode ) => self.TryReadText( charSpan, mode ).OrThrow( newFormatException );
	public static object? ReadVerbatimText( this Codec self, Sys.ReadOnlySpan<char> charSpan ) => self.ReadText( charSpan, Codec.Mode.Verbatim );
	public static object? ReadScriptText( this Codec self, Sys.ReadOnlySpan<char> charSpan ) => self.ReadText( charSpan, Codec.Mode.Script );
	public static string ToVerbatimString<T>( this Codec<T> self, T value ) => self.ToString( value, Codec.Mode.Verbatim );
	public static string ToString<T>( this Codec<T> self, T value, Codec.Mode mode ) => ((Codec)self).ToString( value, mode );
	public static Result<object?, Expectation> TryReadVerbatimText( this Codec self, Sys.ReadOnlySpan<char> charSpan ) => self.TryReadText( charSpan, Codec.Mode.Verbatim );
	public static Result<object?, Expectation> TryReadScriptText( this Codec self, Sys.ReadOnlySpan<char> charSpan ) => self.TryReadText( charSpan, Codec.Mode.Script );
	public static T ReadText<T>( this Codec<T> self, Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode ) => self.TryReadText( charSpan, mode ).OrThrow( newFormatException );
	public static T ReadVerbatimText<T>( this Codec<T> self, Sys.ReadOnlySpan<char> charSpan ) => self.ReadText( charSpan, Codec.Mode.Verbatim );
	public static T ReadScriptText<T>( this Codec<T> self, Sys.ReadOnlySpan<char> charSpan ) => self.TryReadText( charSpan, Codec.Mode.Script ).OrThrow( newFormatException );
	public static void WriteVerbatimText<T>( this Codec<T> self, T value, TextConsumer textConsumer ) => self.WriteText( value, textConsumer, Codec.Mode.Verbatim );
	public static void WriteScriptText<T>( this Codec<T> self, T value, TextConsumer textConsumer ) => self.WriteText( value, textConsumer, Codec.Mode.Script );
	public static Result<T, Expectation> TryReadVerbatimText<T>( this Codec<T> self, Sys.ReadOnlySpan<char> charSpan ) => self.TryReadText( charSpan, Codec.Mode.Verbatim );
	public static Result<T, Expectation> TryReadScriptText<T>( this Codec<T> self, Sys.ReadOnlySpan<char> charSpan ) => self.TryReadText( charSpan, Codec.Mode.Script );
}
