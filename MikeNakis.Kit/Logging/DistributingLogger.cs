namespace MikeNakis.Kit.Logging;

using MikeNakis.Kit.Collections;
using System.Collections.Generic;
using Sys = System;
using SysDiag = System.Diagnostics;

public sealed class DistributingLogger : Logger
{
	public static Logger Of( params Logger[] loggers )
	{
		DistributingLogger distributingLogger = new();
		foreach( Logger logger in loggers )
			distributingLogger.AddLogger( logger );
		return distributingLogger;
	}

	readonly object lockObject = new();
	readonly MutableList<Logger> loggers = new();

	public void AddLogger( Logger logger )
	{
		lock( lockObject )
		{
			Assert( !loggers.Contains( logger ) );
			loggers.Add( logger );
		}
	}

	public void RemoveLog( Logger logger )
	{
		lock( lockObject )
			loggers.DoRemove( logger );
	}

	IReadOnlyList<Logger> getLoggers()
	{
		lock( lockObject )
			return loggers.AsReadOnlyList.Collect();
	}

	public override void AddLogEntry( LogEntry logEntry )
	{
		IReadOnlyList<Logger> loggers = getLoggers();
		if( loggers.Count == 0 )
			throw Failure();
		foreach( Logger logger in loggers )
			try
			{
				logger.AddLogEntry( logEntry );
			}
			catch( Sys.Exception exception )
			{
				foreach( string line in KitHelpers.BuildMediumExceptionMessage( "Logger failed", exception ) )
					SysDiag.Debug.WriteLine( line );
			}
	}
}
