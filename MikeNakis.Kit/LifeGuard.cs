namespace MikeNakis.Kit;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit.Logging;
using Sys = System;
using SysComp = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;
using SysReflect = System.Reflection;

// NOTE: a better name for this class would be "ObjectLifeTimeGuard", but that would be too damn long; hence, "LifeGuard".
[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class LifeGuard : Sys.IDisposable
{
	public static LifeGuard Create( [SysComp.CallerFilePath] string callerFilePath = null!, [SysComp.CallerLineNumber] int callerLineNumber = 0 )
	{
		return Create( framesToSkip: 1, false, callerFilePath, callerLineNumber );
	}

	public static LifeGuard Create( bool collectStackTrace, [SysComp.CallerFilePath] string callerFilePath = null!, [SysComp.CallerLineNumber] int callerLineNumber = 0 )
	{
		return Create( framesToSkip: 1, collectStackTrace, callerFilePath, callerLineNumber );
	}

	public static LifeGuard Create( int framesToSkip, bool collectStackTrace, string callerFilePath, int callerLineNumber )
	{
		Assert( callerFilePath != null );
		if( !DebugMode )
			return ProductionLifeGuard.Instance;
		if( collectStackTrace )
			return new VerboseDebugLifeGuard( callerFilePath, callerLineNumber, framesToSkip + 1 );
		return new TerseDebugLifeGuard( callerFilePath, callerLineNumber );
	}

	public abstract void Dispose();

	public abstract bool IsAliveAssertion();
	public abstract string ToStringFor( Sys.IDisposable owner );
}

sealed class ProductionLifeGuard : LifeGuard
{
	public static readonly ProductionLifeGuard Instance = new();

	ProductionLifeGuard()
	{ } //nothing to do

	public override void Dispose()
	{ } //nothing to do

	public override bool IsAliveAssertion() => throw Failure(); //never invoke on a release build
	public override string ToStringFor( Sys.IDisposable owner ) => throw Failure(); //never invoke on a release build
}

abstract class DebugLifeGuard : LifeGuard
{
	bool alive = true;
	readonly string callerFilePath;
	readonly int callerLineNumber;
	readonly string message;

	protected DebugLifeGuard( string callerFilePath, int callerLineNumber, string message )
	{
		this.callerFilePath = callerFilePath;
		this.callerLineNumber = callerLineNumber;
		this.message = message;
	}

	public sealed override void Dispose()
	{
		Assert( alive );
		alive = false;
		Sys.GC.SuppressFinalize( this );
	}

	[SysDiag.DebuggerHidden]
	public sealed override bool IsAliveAssertion()
	{
		Assert( alive );
		return true;
	}

	protected static string GetSourceInfo( string? filename, int lineNumber ) => $"{filename}({lineNumber})";

	~DebugLifeGuard()
	{
		report( message, callerFilePath, callerLineNumber );
	}

	public override string ToString() => $"{Id( this )} {statusMessage}";
	string statusMessage => alive ? "Alive" : "ENDED";
	public override string ToStringFor( Sys.IDisposable owner ) => $"{Id( owner )} {statusMessage}";

	readonly struct SourceLocation : Sys.IEquatable<SourceLocation>
	{
		readonly string filePath;
		readonly int lineNumber;

		public SourceLocation( string filePath, int lineNumber )
		{
			this.filePath = filePath;
			this.lineNumber = lineNumber;
		}

		[Sys.Obsolete] public override bool Equals( object? other ) => other is SourceLocation kin && Equals( kin );
		public bool Equals( SourceLocation other ) => filePath == other.filePath && lineNumber == other.lineNumber;
		public override int GetHashCode() => Sys.HashCode.Combine( filePath, lineNumber );
	}

	static readonly ICollection<SourceLocation> reportedSourceLocations = new HashSet<SourceLocation>();

	void report( string message, string callerFilePath, int callerLineNumber )
	{
		SourceLocation callerSourceLocation = new( callerFilePath, callerLineNumber );
		lock( reportedSourceLocations )
		{
			if( reportedSourceLocations.Contains( callerSourceLocation ) )
				return;
			reportedSourceLocations.Add( callerSourceLocation );
		}
		Log.LogRawMessage( LogLevel.Error, $"IDisposable allocated at this source location was never disposed!. {message} ({Id( this )})", callerFilePath, callerLineNumber );
		Breakpoint(); //you may resume program execution to see more leaked disposables, but please fix this before committing.
	}
}

sealed class TerseDebugLifeGuard : DebugLifeGuard
{
	public TerseDebugLifeGuard( string callerFilePath, int callerLineNumber )
			: base( callerFilePath, callerLineNumber, $"To enable stack trace collection for this class, pass 'true' to the {nameof( LifeGuard )}.{nameof( Create )}() method call." )
	{ }
}

sealed class VerboseDebugLifeGuard : DebugLifeGuard
{
	public VerboseDebugLifeGuard( string callerFilePath, int callerLineNumber, int framesToSkip )
			: base( callerFilePath, callerLineNumber, buildMessage( framesToSkip + 1 ) )
	{ }

	static string buildMessage( int framesToSkip ) //
		=> "Stack Trace:\r\n" + string.Join( "\r\n", getStackFrames( framesToSkip + 1 ).Select( getSourceInfoFromStackFrame ) );

	static IEnumerable<SysDiag.StackFrame> getStackFrames( int framesToSkip )
	{
		SysDiag.StackTrace stackTrace = new( framesToSkip + 1, true );
		SysDiag.StackFrame[] frames = stackTrace.GetFrames();
		assertTypeOk( frames );
		return frames.Where( f => f.GetFileName() != null );

		static void assertTypeOk( SysDiag.StackFrame[] stackFrames )
		{
			SysReflect.MethodBase? method = stackFrames[0].GetMethod();
			Sys.Type? type = method?.DeclaringType;
			Assert( type != null );
			Assert( typeof( Sys.IDisposable ).IsAssignableFrom( type ) );
		}
	}

	static string getSourceInfoFromStackFrame( SysDiag.StackFrame frame )
	{
		string sourceInfo = GetSourceInfo( frame.GetFileName(), frame.GetFileLineNumber() );
		SysReflect.MethodBase? method = frame.GetMethod();
		return $"    {sourceInfo}: {method?.DeclaringType}.{method?.Name}()";
	}
}
