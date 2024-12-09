namespace MikeNakis.Kit.TextLines;

using MikeNakis.Kit.Logging;

public class LoggingTextLineWriter : TextLineWriter
{
	readonly Logger logger;
	readonly LogLevel logLevel;

	public LoggingTextLineWriter( Logger logger, LogLevel logLevel )
	{
		this.logger = logger;
		this.logLevel = logLevel;
	}

	public void WriteLine( string text )
	{
		LogEntry entry = new( logLevel, DotNetHelpers.GetWallClockTime(), text, "", 0 );
		logger.AddLogEntry( entry );
	}
}
