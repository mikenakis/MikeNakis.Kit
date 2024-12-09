namespace MikeNakis.Kit.Logging;

public abstract class Logger
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
	public static Logger Instance = DebugLogger.Instance; //by default, we only have a debug logger; the application may replace this with a more elaborate logger.
#pragma warning restore CA2211 // Non-constant fields should not be visible

	public abstract void AddLogEntry( LogEntry logEntry );
}
