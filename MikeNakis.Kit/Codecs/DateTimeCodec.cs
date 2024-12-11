namespace MikeNakis.Kit.Codecs;

using MikeNakis.Kit;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysGlob = System.Globalization;

/// A <see cref="Codec{T}" /> for <see cref="Sys.DateTime" />.
public sealed class DateTimeCodec : AbstractCodec<Sys.DateTime>
{
	public const string Format = "yyyy-MM-dd HH:mm:ss.fffffff";

	public static readonly DateTimeCodec Instance = new();

	DateTimeCodec() { }

	public override void WriteText( Sys.DateTime value, TextConsumer textConsumer, Codec.Mode mode )
	{
		Sys.Span<char> destination = stackalloc char[27];
		bool ok = value.TryFormat( destination, out int charsWritten, Format, provider: SysGlob.CultureInfo.InvariantCulture );
		Assert( ok );
		textConsumer.Invoke( destination[..charsWritten] );
	}

	public override Result<Sys.DateTime, Expectation> TryReadText( Sys.ReadOnlySpan<char> charSpan, Codec.Mode mode )
	{
		return mode switch
		{
			Codec.Mode.Verbatim => fromStringVerbatim( charSpan ),
			Codec.Mode.Script => fromStringScript( charSpan ),
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( mode ), mode, null )
		};

		static Result<Sys.DateTime, Expectation> fromStringVerbatim( Sys.ReadOnlySpan<char> charSpan )
		{
			if( !Sys.DateTime.TryParseExact( charSpan, Format, SysGlob.CultureInfo.InvariantCulture, SysGlob.DateTimeStyles.None, out Sys.DateTime value ) )
				return Result<Sys.DateTime, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a date-time." ) );
			return Result<Sys.DateTime, Expectation>.Success( value );
		}

		static Result<Sys.DateTime, Expectation> fromStringScript( Sys.ReadOnlySpan<char> charSpan )
		{
			Assert( false ); //the following is probably incorrect. We might need a date-time format for date-time literals in script.
			if( !Sys.DateTime.TryParseExact( charSpan, Format, SysGlob.CultureInfo.InvariantCulture, SysGlob.DateTimeStyles.None, out Sys.DateTime value ) )
				return Result<Sys.DateTime, Expectation>.Failure( new CustomExpectation( $"could not parse '{charSpan}' as a date-time." ) );
			return Result<Sys.DateTime, Expectation>.Success( value );
		}
	}

	public override void WriteBinary( Sys.DateTime value, BinaryStreamWriter binaryStreamWriter ) => Instant.FromDateTime( value ).ToOutputStream( binaryStreamWriter );
	public override Sys.DateTime ReadBinary( BinaryStreamReader binaryStreamReader ) => Instant.FromInputStream( binaryStreamReader ).ToDateTime();
}
