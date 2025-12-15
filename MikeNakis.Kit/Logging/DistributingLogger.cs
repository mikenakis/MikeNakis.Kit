namespace MikeNakis.Kit.Logging;

using System.Collections.Generic;
using MikeNakis.Kit.Collections;
using static MikeNakis.Kit.GlobalStatics;
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

	readonly MutableList<Logger> loggers = new();

	public void AddLogger( Logger logger )
	{
		Assert( !loggers.Contains( logger ) );
		loggers.Add( logger );
	}

	public void RemoveLog( Logger logger )
	{
		loggers.DoRemove( logger );
	}

	IReadOnlyList<Logger> getLoggers()
	{
		return loggers.AsReadOnlyList.Collect();
	}

	protected override void OnAddLogEntry( LogEntry logEntry )
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
