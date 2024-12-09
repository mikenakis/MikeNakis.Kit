namespace MikeNakis.Kit;

using Sys = System;

///<summary>Provides access to the current time.</summary>
///<remarks>To be used in place of DotNet's dumb-as-fuck System.DateTime.Now.</remarks>
public interface Clock
{
	Sys.DateTime GetUniversalTime();
	Sys.TimeZoneInfo GetLocalTimeZone();
}
