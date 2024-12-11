namespace MikeNakis.Kit;
using MikeNakis.Kit.Codecs;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

public readonly record struct Instant
{
	const double dateTimeTicksPerSecond = 10000000.0; // A DateTime tick is a 100-nanosecond interval.

	public static Instant FromDateTime( Sys.DateTime dateTime )
	{
		Assert( dateTime.Kind == Sys.DateTimeKind.Utc );
		double epochSeconds = dateTime.Ticks / dateTimeTicksPerSecond;
		return new Instant( epochSeconds );
	}

	public static Instant FromInputStream( BinaryStreamReader binaryStreamReader )
	{
		double epochSeconds = Float64Codec.Instance.ReadBinary( binaryStreamReader );
		return new Instant( epochSeconds );
	}

	readonly double epochSeconds;

	public Instant( double epochSeconds )
	{
		this.epochSeconds = epochSeconds;
	}

	public Sys.DateTime ToDateTime()
	{
		return new Sys.DateTime( (long)(epochSeconds * dateTimeTicksPerSecond), Sys.DateTimeKind.Utc );
	}

	public void ToOutputStream( BinaryStreamWriter binaryStreamWriter )
	{
		Float64Codec.Instance.WriteBinary( epochSeconds, binaryStreamWriter );
	}
}
