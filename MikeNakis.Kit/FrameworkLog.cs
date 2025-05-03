namespace MikeNakis.Kit;

using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.Logging;

public static class Log
{
	public static void Debug( string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Debug, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	public static void Info( string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Info, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	public static void Warn( string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	public static void Error( string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( LogLevel.Error, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	public static void MessageWithGivenLevel( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> fixAndLogMessage( logLevel, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	public static void Warn( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Warn, DotNetHelpers.GetWallClockTime(), buildLongExceptionMessage( prefix, exception ), sourceFileName, sourceLineNumber );

	public static void Error( string prefix, Sys.Exception exception, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( LogLevel.Error, DotNetHelpers.GetWallClockTime(), buildLongExceptionMessage( prefix, exception ), sourceFileName, sourceLineNumber );

	public static void LogRawMessage( LogLevel logLevel, string message, //
			[SysCompiler.CallerFilePath] string sourceFileName = "", [SysCompiler.CallerLineNumber] int sourceLineNumber = 0 ) //
		=> logRawMessage( logLevel, DotNetHelpers.GetWallClockTime(), message, sourceFileName, sourceLineNumber );

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static void fixAndLogMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFileName, int sourceLineNumber )
	{
		message = fixMessage( message );
		logRawMessage( logLevel, utc, message, sourceFileName, sourceLineNumber );
	}

	static void logRawMessage( LogLevel logLevel, Sys.DateTime utc, string message, string sourceFileName, int sourceLineNumber )
	{
		sourceFileName = fixSourceFileName( sourceFileName );
		LogEntry entry = new( logLevel, utc, message, sourceFileName, sourceLineNumber );
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

	static string fixSourceFileName( string sourceFileName )
	{
		string solutionSourcePath = SolutionSourcePath.Value;
		if( !sourceFileName.StartsWith2( solutionSourcePath ) )
			return sourceFileName;
		int start = solutionSourcePath.Length;
		while( start < sourceFileName.Length && (sourceFileName[start] == '\\' || sourceFileName[start] == '/') )
			start++;
		//PEARL: The "..\" prefix is necessary, otherwise Visual Studio exhibits some incredibly buggy behavior.
		//       For more information about this, see https://stackoverflow.com/q/75224235/773113
		return "..\\" + sourceFileName[start..];
	}
}
