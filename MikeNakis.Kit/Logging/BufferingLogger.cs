namespace MikeNakis.Kit.Logging;

using System.Collections.Generic;
using MikeNakis.Kit.Collections;

public class BufferingLogger : Logger
{
	MutableList<LogEntry> logEntries = new();

	public BufferingLogger()
	{ }

	protected override void OnAddLogEntry( LogEntry logEntry )
	{
		logEntries.Add( logEntry );
	}

	public void ReplayAndClear( Logger log )
	{
		IReadOnlyList<LogEntry> tempLogEntries;
		tempLogEntries = logEntries.AsReadOnlyList;
		logEntries = new MutableList<LogEntry>();
		foreach( LogEntry logEntry in tempLogEntries )
			log.AddLogEntry( logEntry );
	}

	public IReadOnlyList<LogEntry> GetEntriesAndClear()
	{
		IReadOnlyList<LogEntry> tempLogEntries;
		tempLogEntries = logEntries.AsReadOnlyList;
		logEntries = new MutableList<LogEntry>();
		return tempLogEntries;
	}
}
