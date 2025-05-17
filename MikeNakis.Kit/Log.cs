namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.FileSystem;
using MikeNakis.Kit.Logging;

public static class Log
{
	public static void Debug( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Debug, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Info( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Info, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Warn( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Error( string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Error, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void MessageWithGivenLevel( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( logLevel, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Warn( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTime(), buildLongExceptionMessage( prefix, exception ), sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void Error( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Error, DotNetHelpers.GetWallClockTime(), buildLongExceptionMessage( prefix, exception ), sourceFilePathName.OrThrow(), sourceLineNumber );

	public static void LogRawMessage( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string? sourceFilePathName = null, [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( logLevel, DotNetHelpers.GetWallClockTime(), message, sourceFilePathName.OrThrow(), sourceLineNumber );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static DirectoryPath? startupProjectDirectoryPath;

	public static void SetStartupProjectDirectoryPathName( string directoryPathName )
	{
		if( startupProjectDirectoryPath != null )
		{
			Warn( $"Ignoring Startup Project Directory Path '{directoryPathName}'" );
			return;
		}
		startupProjectDirectoryPath = DirectoryPath.FromAbsolutePath( directoryPathName );
	}

	static void fixAndLogMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFileName, int sourceLineNumber )
	{
		message = fixMessage( message );
		logRawMessage( logLevel, utc, message, sourceFileName, sourceLineNumber );
	}

	static void logRawMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFilePathName, int sourceLineNumber )
	{
		sourceFilePathName = fixSourceFileName( sourceFilePathName );
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

	static bool reported;

	static string fixSourceFileName( string sourceFilePathName )
	{
		if( startupProjectDirectoryPath != null && FileSystemPath.IsAbsolute( sourceFilePathName ) )
		{
			if( !reported )
			{
				reported = true;
				Info( $"Startup Project Directory Path: {startupProjectDirectoryPath}" );
			}
			FilePath sourceFilePath = FilePath.FromAbsolutePath( sourceFilePathName );
			return startupProjectDirectoryPath.GetRelativePath( sourceFilePath );
		}
		return sourceFilePathName;
	}
}
