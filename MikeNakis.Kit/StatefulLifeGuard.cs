namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

public sealed class StatefulLifeGuard : Sys.IDisposable
{
	public static StatefulLifeGuard Create( bool collectStackTrace = false, //
			[SysCompiler.CallerFilePath] string callerFilePath = null!, [SysCompiler.CallerLineNumber] int callerLineNumber = 0 )
	{
		return new StatefulLifeGuard( collectStackTrace, 1, callerFilePath, callerLineNumber );
	}

	readonly LifeGuard lifeGuard;
	public bool IsAlive { get; private set; } = true;

	StatefulLifeGuard( bool collectStackTrace, int framesToSkip, string callerFilePath, int callerLineNumber = 0 )
	{
		lifeGuard = LifeGuard.Create( framesToSkip + 1, collectStackTrace, callerFilePath, callerLineNumber );
	}

	public void Dispose()
	{
		lifeGuard.Dispose();
		IsAlive = false;
	}

	[SysDiag.DebuggerHidden]
	public bool IsAliveAssertion()
	{
		Assert( IsAlive );
		return true;
	}

	public override string? ToString() => lifeGuard.ToString();
}
