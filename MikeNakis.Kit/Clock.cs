namespace MikeNakis.Kit;

///<summary>Provides access to the current time.</summary>
///<remarks>To be used in place of DotNet's ill-conceived System.DateTime.Now and UtcNow.</remarks>
public interface Clock
{
	Sys.DateTime GetUniversalTime();
	Sys.TimeZoneInfo GetLocalTimeZone();
}
