namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.Logging;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;

public static class Log
{
	public static void Debug( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Debug, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Info( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Info, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Warn( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Error( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Error, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void MessageWithGivenLevel( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( logLevel, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Warn( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTimeUtc(), buildLongExceptionMessage( prefix, exception ), sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Error( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Error, DotNetHelpers.GetWallClockTimeUtc(), buildLongExceptionMessage( prefix, exception ), sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void LogRawMessage( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( logLevel, DotNetHelpers.GetWallClockTimeUtc(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void fixAndLogMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFileName, int sourceLineNumber )
	{
		message = fixMessage( message );
		logRawMessage( logLevel, utc, message, sourceFileName, sourceLineNumber );
	}

	static void logRawMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFilePathName, int sourceLineNumber )
	{
		sourceFilePathName = StartupProjectDirectory.MakeRelative( sourceFilePathName );
		LogEntry entry = new( logLevel, utc, message, sourceFilePathName, sourceLineNumber );
		Logger.Instance.AddLogEntry( entry );
	}

	static string fixMessage( string message )
	{
		message = message.Replace2( "|", "¦" );
		message = message.Replace2( "\r\n", " ¦ " );
		message = message.Replace2( "\r", " ¦ " );
		message = message.Replace2( "\n", " ¦ " );
		message = message.Replace2( "\t", "    " );
		return message;
	}

	static string buildLongExceptionMessage( string prefix, Sys.Exception exception ) => KitHelpers.BuildLongExceptionMessage( prefix, exception ).MakeString( "\r\n" );
}
