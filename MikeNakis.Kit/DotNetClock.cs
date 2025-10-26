namespace MikeNakis.Kit;

public class DotNetClock : Clock
{
	public static Clock Instance { get; } = new DotNetClock();

	DotNetClock()
	{ }

	public Sys.DateTime GetUniversalTime() => DotNetHelpers.GetWallClockTimeUtc();
	public Sys.TimeZoneInfo GetLocalTimeZone() => Sys.TimeZoneInfo.Local;
}
