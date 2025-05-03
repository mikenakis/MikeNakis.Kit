namespace MikeNakis.Kit.Logging;

public class LogEntry
{
	public LogLevel LogLevel { get; }
	public Sys.DateTime Utc { get; }
	public string Message { get; }
	public string SourceFileName { get; }
	public int SourceLineNumber { get; }

	public LogEntry( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFileName, int sourceLineNumber )
	{
		LogLevel = logLevel;
		Utc = utc;
		Message = message;
		SourceFileName = sourceFileName;
		SourceLineNumber = sourceLineNumber;
	}

	public override string ToString()
	{
		return $"level={LogLevel}; utc={Utc}; message={Message}; sourceFileName={SourceFileName}; sourceLineNumber={SourceLineNumber}";
	}

	public IReadOnlyList<string> ToStrings()
	{
		Sys.DateTime t = Utc.ToLocalTime();
		return ReadOnlyListOf( //
			$"{SourceFileName}({SourceLineNumber}): ", //
			$"{StringFromLogLevel( LogLevel )}", //
			$" | {t.Year:D4}-{t.Month:D2}-{t.Day:D2} {t.Hour:D2}:{t.Minute:D2}:{t.Second:D2}.{t.Millisecond:D3} | ", //
			Message );
	}

	public static string StringFromLogLevel( LogLevel logLevel )
	{
		return logLevel switch
		{
			LogLevel.Debug => "DEBUG",
			LogLevel.Info => "INFO ",
			LogLevel.Warn => "WARN ",
			LogLevel.Error => "ERROR",
			_ => "unknown:" + logLevel
		};
	}
}
