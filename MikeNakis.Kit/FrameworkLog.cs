namespace MikeNakis.Kit;

using System.Linq;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.Logging;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;
using SysReflect = System.Reflection;
using SysText = System.Text;

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

	static string buildLongExceptionMessage( string prefix, Sys.Exception exception )
	{
		MutableList<string> lines = new();
		recurse( $"{prefix} : ", lines, exception );
		return lines.AsReadOnlyList.Select( fixMessage ).MakeString( "\r\n" );

		static void recurse( string prefix, MutableList<string> lines, Sys.Exception exception )
		{
			for( ; true; exception = exception.InnerException )
			{
				lines.Add( $"{prefix}{exception.GetType()} : {exception.Message}" );
				SysDiag.StackFrame[] stackFrames = new SysDiag.StackTrace( exception, true ).GetFrames();
				lines.AddRange( stackFrames.Select( stringFromStackFrame ) );
				if( exception is Sys.AggregateException aggregateException )
				{
					Assert( ReferenceEquals( exception.InnerException, aggregateException.InnerExceptions[0] ) );
					foreach( Sys.Exception innerException in aggregateException.InnerExceptions )
						recurse( "Aggregates ", lines, innerException );
					break;
				}
				if( exception.InnerException == null )
					break;
				prefix = "Caused by ";
			}
		}
	}

	static string stringFromStackFrame( SysDiag.StackFrame stackFrame )
	{
		SysText.StringBuilder stringBuilder = new();
		stringBuilder.Append( "    " );
		string? sourceFileName = stackFrame.GetFileName();
		stringBuilder.Append( string.IsNullOrEmpty( sourceFileName ) ? "<unknown-source>: " : $"{fixSourceFileName( sourceFileName )}({stackFrame.GetFileLineNumber()}): " );
		SysReflect.MethodBase? method = stackFrame.GetMethod();
		if( method != null )
		{
			stringBuilder.Append( "method " );
			Sys.Type? declaringType = method.DeclaringType;
			if( declaringType != null )
				stringBuilder.Append( KitHelpers.GetCSharpTypeName( declaringType ).Replace( '+', '.' ) ).Append( '.' );
			stringBuilder.Append( method.Name );
			if( method is SysReflect.MethodInfo && method.IsGenericMethod )
				stringBuilder.Append( '<' ).Append( method.GetGenericArguments().Select( a => a.Name ).MakeString( "," ) ).Append( '>' );
			stringBuilder.Append( '(' ).Append( method.GetParameters().Select( p => p.ParameterType.Name + " " + p.Name ).MakeString( ", " ) ).Append( ')' );
		}
		return stringBuilder.ToString();
	}

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
