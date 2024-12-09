namespace MikeNakis.Kit.Logging;

using MikeNakis.Kit.Collections;

public class BufferingLogger : Logger
{
	readonly object myLock = new();
	MutableList<LogEntry> logEntries = new();

	public BufferingLogger()
	{ }

	public override void AddLogEntry( LogEntry logEntry )
	{
		lock( myLock )
			logEntries.Add( logEntry );
	}

	public void ReplayAndClear( Logger log )
	{
		IReadOnlyList<LogEntry> tempLogEntries;
		lock( myLock )
		{
			tempLogEntries = logEntries.AsReadOnlyList;
			logEntries = new MutableList<LogEntry>();
		}
		foreach( LogEntry logEntry in tempLogEntries )
			log.AddLogEntry( logEntry );
	}

	public IReadOnlyList<LogEntry> GetEntriesAndClear()
	{
		IReadOnlyList<LogEntry> tempLogEntries;
		lock( myLock )
		{
			tempLogEntries = logEntries.AsReadOnlyList;
			logEntries = new MutableList<LogEntry>();
		}
		return tempLogEntries;
	}
}
