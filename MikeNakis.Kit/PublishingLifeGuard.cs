namespace MikeNakis.Kit;

using MikeNakis.Kit.Events;
using Sys = System;
using SysComp = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public class PublishingLifeGuard : Sys.IDisposable
{
	public static PublishingLifeGuard Create( [SysComp.CallerFilePath] string callerFilePath = null!, [SysComp.CallerLineNumber] int callerLineNumber = 0 )
	{
		return Create( framesToSkip: 1, false, callerFilePath, callerLineNumber );
	}

	public static PublishingLifeGuard Create( bool collectStackTrace, [SysComp.CallerFilePath] string callerFilePath = null!, [SysComp.CallerLineNumber] int callerLineNumber = 0 )
	{
		return Create( framesToSkip: 1, collectStackTrace, callerFilePath, callerLineNumber );
	}

	public static PublishingLifeGuard Create( int framesToSkip, bool collectStackTrace, string callerFilePath, int callerLineNumber )
	{
		return new PublishingLifeGuard( framesToSkip, collectStackTrace, callerFilePath, callerLineNumber );
	}

	readonly LifeGuard lifeGuard;
	readonly MutablePublisher<Unit> publisher = new();
	public Publisher<Unit> Publisher => publisher;

	PublishingLifeGuard( int framesToSkip, bool collectStackTrace, string callerFilePath, int callerLineNumber )
	{
		lifeGuard = LifeGuard.Create( framesToSkip, collectStackTrace, callerFilePath, callerLineNumber );
	}

	public void Dispose()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		publisher.Publish( Unit.Instance );
		publisher.Dispose();
		lifeGuard.Dispose();
	}

	public bool IsAliveAssertion() => lifeGuard.IsAliveAssertion();
	public string ToStringFor( Sys.IDisposable owner ) => lifeGuard.ToStringFor( owner );
}
